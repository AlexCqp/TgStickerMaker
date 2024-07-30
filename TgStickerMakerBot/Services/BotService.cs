using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using TgStickerMaker.Logger;
using TgStickerMaker.MediaLoading;
using TgStickerMaker;
using static TgStickerMaker.ServiceConfiguration;
using Telegram.Bot.Types.ReplyMarkups;
using TgStickerMakerBot.Models;

namespace TgStickerMakerBot.Services
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;

        // Словари для хранения состояния каждого пользователя
        private readonly List<UserState> UserStates = new();

        private readonly UserState _currentUserState;

        public BotService(string botToken)
        {
            _botClient = new TelegramBotClient(botToken);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"Бот {me.FirstName} запущен. Нажмите Ctrl+C для завершения работы");

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                },
                cancellationToken: cancellationToken
            );

            // Just wait until cancellation token is triggered
            await Task.Delay(Timeout.Infinite, cancellationToken);

            Console.WriteLine("Бот остановлен");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { } message)
            {
                var chatId = message.Chat.Id;

                try
                {
                    if (message.Type == MessageType.Text)
                    {
                        await HandleTextMessageAsync(message, chatId, cancellationToken);
                    }
                    else if (message.Document is not null || message.Photo is not null || message.Video is not null)
                    {
                        await HandleMediaMessageAsync(message, chatId, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    await _botClient.SendTextMessageAsync(chatId, "Ошибка", cancellationToken: cancellationToken);
                }
            }
            else if (update.CallbackQuery is { } callbackQuery)
            {
                var chatId = callbackQuery.Message.Chat.Id;
                await HandleCallbackQueryAsync(callbackQuery, chatId, cancellationToken);
            }
            else if (update.Message is { } commandMessage && commandMessage.Type == MessageType.Text)
            {
                var command = commandMessage.Text.Split(' ')[0];
                if (command == "/start")
                {
                    await SendWelcomeMessageAsync(commandMessage.Chat.Id, cancellationToken);
                }
                else if (command == "/createSticker")
                {
                    await StartStickerCreationAsync(commandMessage.Chat.Id, cancellationToken);
                }
            }
        }

        private async Task HandleTextMessageAsync(Message message, long chatId, CancellationToken cancellationToken)
        {
            var text = message.Text;

            if (UserStates.All(x => x.UserId != chatId))
            {
                UserStates.Add(new UserState(chatId));
            }

            var currentStep = GetUserState(chatId).CurrentStep;

            switch (currentStep)
            {
                case 0:
                    if ((text.Contains("http://") || text.Contains("https://")) && Uri.TryCreate(text, UriKind.Absolute, out _))
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Ссылка определена, идет загрузка файла...", cancellationToken: cancellationToken);
                        GetUserState(chatId).FilePath = await MediaLoader.LoadMediaFrom(text);
                        await SendDurationPrompt(chatId, cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Отправьте файл или ссылку на файл.", cancellationToken: cancellationToken);
                    }
                    break;
                case 1:
                    if (int.TryParse(text, out var videoDuration))
                    {
                        GetUserState(chatId).VideoDuration = videoDuration;
                    }
                    else
                    {
                        GetUserState(chatId).VideoDuration = 0;
                    }
                    await SendTopTextPrompt(chatId, cancellationToken);
                    break;
                case 2:
                    GetUserState(chatId).TopText = text;
                    await SendBottomTextPrompt(chatId, cancellationToken);
                    break;
                case 3:
                    GetUserState(chatId).BottomText = text;
                    await ProcessFile(GetUserState(chatId).FilePath, chatId, cancellationToken);
                    GetUserState(chatId).CurrentStep = 0;
                    break;
            }
        }

        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, long chatId, CancellationToken cancellationToken)
        {
            if (GetUserState(chatId) == null)
            {
                UserStates.Add(new UserState(chatId));
            }

            var currentStep = GetUserState(chatId).CurrentStep;

            if (callbackQuery.Data == "skip")
            {
                switch (currentStep)
                {
                    case 0:
                        await _botClient.SendTextMessageAsync(chatId, "Отправьте файл или ссылку на файл.", cancellationToken: cancellationToken);
                        break;
                    case 1:
                        await SendTopTextPrompt(chatId, cancellationToken);
                        break;
                    case 2:
                        await SendBottomTextPrompt(chatId, cancellationToken);
                        break;
                    case 3:
                        await ProcessFile(GetUserState(chatId).FilePath, chatId, cancellationToken);
                        GetUserState(chatId).CurrentStep = 0;
                        break;
                }
            }
            else if (callbackQuery.Data == "back")
            {
                switch (currentStep)
                {
                    case 1:
                        GetUserState(chatId).CurrentStep = 0;
                        await _botClient.SendTextMessageAsync(chatId, "Отправьте файл или ссылку на файл.", cancellationToken: cancellationToken);
                        break;
                    case 2:
                        GetUserState(chatId).CurrentStep = 1;
                        await SendDurationPrompt(chatId, cancellationToken);
                        break;
                    case 3:
                        GetUserState(chatId).CurrentStep = 2;
                        await SendTopTextPrompt(chatId, cancellationToken);
                        break;
                }
            }
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
        }

        private async Task HandleMediaMessageAsync(Message message, long chatId, CancellationToken cancellationToken)
        {
            var fileId = message.Type switch
            {
                MessageType.Document => message.Document.FileId,
                MessageType.Photo => message.Photo[^1].FileId, // Берем последнюю, т.е. самую высокую по качеству
                MessageType.Video => message.Video.FileId,
                _ => throw new ArgumentOutOfRangeException("неверный тип файла")
            };

            var file = await _botClient.GetFileAsync(fileId, cancellationToken);
            var filePath = $"downloads/{file.FileId}.{file.FilePath.Split('.').Last()}";
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var saveStream = new FileStream(filePath, FileMode.Create))
            {
                await _botClient.DownloadFileAsync(file.FilePath, saveStream, cancellationToken);
            }

            GetUserState(chatId).FilePath = filePath;
            await SendDurationPrompt(chatId, cancellationToken);
        }

        private async Task SendWelcomeMessageAsync(long chatId, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Создать стикер", "createSticker") }
            });

            await _botClient.SendTextMessageAsync(chatId, "Привет! Нажмите кнопку, чтобы начать создание стикера.", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private async Task StartStickerCreationAsync(long chatId, CancellationToken cancellationToken)
        {
            GetUserState(chatId).CurrentStep = 0; // Начать с нулевого шага

            await _botClient.SendTextMessageAsync(chatId, "Отправьте файл или ссылку на файл.", cancellationToken: cancellationToken);
        }

        private async Task SendDurationPrompt(long chatId, CancellationToken cancellationToken)
        {
            GetUserState(chatId).CurrentStep = 1;
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Пропустить", "skip") }
            });
            await _botClient.SendTextMessageAsync(chatId, "Введите продолжительность видео (в секундах) или оставьте пустым:", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private async Task SendTopTextPrompt(long chatId, CancellationToken cancellationToken)
        {
            GetUserState(chatId).CurrentStep = 2;
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Пропустить", "skip") }
            });
            await _botClient.SendTextMessageAsync(chatId, "Введите текст сверху (или оставьте пустым):", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private async Task SendBottomTextPrompt(long chatId, CancellationToken cancellationToken)
        {
            GetUserState(chatId).CurrentStep = 3;
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Пропустить", "skip") }
            });
            await _botClient.SendTextMessageAsync(chatId, "Введите текст снизу (или оставьте пустым):", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
        }

        private async Task ProcessFile(string filePath, long chatId, CancellationToken cancellationToken)
        {
            string outputFilePath = null;
            string upscaledFilePath = null;

            try
            {
                if (!Directory.Exists(Settings.OutputDirectory))
                {
                    Directory.CreateDirectory(Settings.OutputDirectory);
                }

                if (StickerMaker.IsImage(filePath))
                {
                    outputFilePath = await StickerMaker.ProcessImage(filePath, GetUserState(chatId).TopText, GetUserState(chatId).BottomText, Settings.BotSettings.OutputDirectory);
                }
                else
                {
                    outputFilePath = await StickerMaker.ProcessVideo(filePath, GetUserState(chatId).TopText, GetUserState(chatId).BottomText, GetUserState(chatId).VideoDuration, Settings.BotSettings.OutputDirectory);
                    // string video2xPath = @"C:\path\to\video2x.exe"; // Укажите путь к экзешнику video2x
                    // upscaledFilePath = await VideoUpscaler.UpscaleVideoAsync(outputFilePath, GetUpscaledFilePath(outputFilePath));
                }

                string finalFilePath = upscaledFilePath ?? outputFilePath;

                using (var stream = new FileStream(finalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var fileName = Path.GetFileName(finalFilePath);
                    await _botClient.SendDocumentAsync(chatId, new InputFileStream(stream, fileName), cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Произошла ошибка при обработке", ex);
                await _botClient.SendTextMessageAsync(chatId, "Произошла ошибка при обработке файла.", cancellationToken: cancellationToken);
            }
            finally
            {
                ClearUserOutput();
            }
        }

        private void ClearUserOutput()
        {
            Logger.LogInfo("Очищаем папку пользователей");
            var directoryInfo = new DirectoryInfo(Settings.BotSettings.OutputDirectory);
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                file.Delete();
            }
        }

        private string GetUpscaledFilePath(string originalFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(originalFilePath), Path.GetFileNameWithoutExtension(originalFilePath) + "_upscaled.webm");
        }

        private UserState? GetUserState(long userId)
            => UserStates.SingleOrDefault(x => x.UserId == userId);
    }
}

using Newtonsoft.Json;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgStickerMaker;
using TgStickerMaker.MediaLoading;
using TgStickerMakerBot.Enums.CommandSteps;
using TgStickerMakerBot.Models;
using TgStickerMakerBot.Models.CallbackModels;
using static TgStickerMakerBot.Buttons.Buttons;

namespace TgStickerMakerBot.Comands
{
    public class CreateStickerComand : IBotCommand
    {
        private readonly ITelegramBotClient _botClient;

        public CreateStickerComand(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task StartAsync(UserState userState, CancellationToken cancellationToken)
        {
            userState.CurrentStep = 0;
            userState.CurrentCommandClass = CommandClass.CreateSticker;
            await _botClient.SendTextMessageAsync(userState.UserId, "Отправьте файл или ссылку на файл (например, https://tenor.com/ru/view/sus-suspicious-gif-23068645).", cancellationToken: cancellationToken);
        }

        public async Task HandleTextMessageAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
            var messageText = message.Text;
            switch ((CreateStickerSteps)userState.CurrentStep)
            {
                case CreateStickerSteps.LoadMedia:
                    {
                        if (!string.IsNullOrEmpty(messageText) && (IsLink(messageText)) && Uri.TryCreate(messageText, UriKind.Absolute, out _))
                        {
                            await _botClient.SendTextMessageAsync(userState.UserId, "Ссылка определена, идет загрузка файла...", cancellationToken: cancellationToken);
                            userState.FilePath = await MediaLoader.LoadMediaFrom(messageText);
                            userState.CurrentStep++;
                            await SendTextPromptWithSkipBackAsync(userState.UserId, "Введите продолжительность видео (в секундах) или оставьте пустым", CreateStickerSteps.SetDuration, null, cancellationToken);
                        }
                        else
                        {
                            Test(message, userState, cancellationToken);
                        }

                        break;
                    }
                case CreateStickerSteps.SetDuration:
                    {
                        if (int.TryParse(messageText, out var videoDuration))
                        {
                            userState.VideoDuration = videoDuration;
                        }
                        else
                        {
                            userState.VideoDuration = default;
                        }

                        userState.CurrentStep++;
                        SendTextPromptWithSkipBackAsync(userState.UserId, "Введите текст сверху (или оставьте пустым)", CreateStickerSteps.SetTopText, CreateStickerSteps.LoadMedia, cancellationToken);
                        
                        break;
                    }
                case CreateStickerSteps.SetTopText:
                    {
                        userState.CurrentStep++;
                        userState.TopText = messageText;
                        await SendTextPromptWithSkipBackAsync(userState.UserId, "Введите текст снизу (или оставьте пустым)", CreateStickerSteps.SetBottomText, CreateStickerSteps.SetTopText, cancellationToken);
                        
                        break;
                    }
                case CreateStickerSteps.SetBottomText:
                    {
                        userState.BottomText = messageText;
                        await CreateSticker(userState, cancellationToken);
                        userState.CurrentStep = (int)CreateStickerSteps.LoadMedia;
                        break;
                    }
                default:
                    break;
            }
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, long userId, CancellationToken cancellationToken, UserState userState)
        {
            var callbackModel = JsonConvert.DeserializeObject<CommandCallbackModel>(callbackQuery.Data);

                switch ((CreateStickerSteps)callbackModel.CurrentStep)
                {
                    case CreateStickerSteps.LoadMedia:
                        userState.CurrentStep++;
                        await _botClient.SendTextMessageAsync(userId, "Отправьте файл или ссылку на файл.", cancellationToken: cancellationToken);
                        break;
                    case CreateStickerSteps.SetDuration:
                        userState.CurrentStep++;
                        await SendTopTextPromptAsync(userId, cancellationToken);
                        break;
                    case CreateStickerSteps.SetTopText:
                        userState.CurrentStep++;
                        await SendBottomTextPromptAsync(userId, cancellationToken);
                        break;
                    case CreateStickerSteps.SetBottomText:
                    {
                        await CreateSticker(userState, cancellationToken);
                        userState.CurrentStep = (int)CreateStickerSteps.LoadMedia;
                        break;
                    }
                }
        }

        public async Task SendCurrentStep(UserState userState, CancellationToken cancellationToken)
        {
            switch ((CreateStickerSteps)userState.CurrentStep - 1)
            {
                case CreateStickerSteps.LoadMedia:
                    await _botClient.SendTextMessageAsync(userState.UserId, "Отправьте файл или ссылку на файл.", cancellationToken: cancellationToken);
                    break;
                case CreateStickerSteps.SetDuration:
                    break;
                case CreateStickerSteps.SetTopText:
                    break;
                case CreateStickerSteps.SetBottomText:
                    break;
                default:
                    break;
            }
        }

        private async Task CreateSticker(UserState userState, CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync(userState.UserId, "Ожидайте завершения обработки...", cancellationToken: cancellationToken);
            var outputPath = await StickerMaker.ProcessFileAsync(userState.FilePath, userState.VideoDuration, userState.TopText, userState.BottomText, TgStickerMaker.ServiceConfiguration.Settings.BotSettings.OutputDirectory);
            using (var stream = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var fileName = Path.GetFileName(outputPath);
                await _botClient.SendDocumentAsync(userState.UserId, new InputFileStream(stream, fileName), cancellationToken: cancellationToken);
            }
        }

        private async Task SendBottomTextPromptAsync(long userId, CancellationToken cancellationToken)
        {
            await SendTextPromptWithSkipBackAsync(userId, "Введите текст снизу (или оставьте пустым)", CreateStickerSteps.SetBottomText, CreateStickerSteps.LoadMedia, cancellationToken);
        }

        private async Task SendTopTextPromptAsync(long userId, CancellationToken cancellationToken)
        {
            await SendTextPromptWithSkipBackAsync(userId, "Введите текст сверху (или оставьте пустым)", CreateStickerSteps.SetTopText, CreateStickerSteps.LoadMedia, cancellationToken);
        }

        private async Task SendTextPromptWithSkipBackAsync(long userId, string message, CreateStickerSteps? nextStep, CreateStickerSteps? previosSteep, CancellationToken cancellationToken)
        {
            var buttons = GetSkipBackStepButtonsWithCallbackData(
                                new CommandCallbackModel()
                                {
                                    CommandClass = CommandClass.CreateSticker,
                                    CurrentStep = (int?)nextStep
                                },
                                new CommandCallbackModel()
                                {
                                    CommandClass = CommandClass.CreateSticker,
                                    CurrentStep = (int?)previosSteep
                                });

            await _botClient.SendTextMessageAsync(userId, message, replyMarkup: buttons, cancellationToken: cancellationToken);
        } 

        private bool IsLink(string messageText)
        {
            return messageText.Contains("http://") || messageText.Contains("https://");
        }

        private async void Test(Message message, UserState userState, CancellationToken cancellationToken)
        {
            if (message.Type == MessageType.Text)
            {
                await HandleTextMessageAsync(message, userState, cancellationToken);
            }
            else if (message.Document is not null || message.Photo is not null || message.Video is not null)
            {
                await HandleMediaMessageAsync(message, userState.UserId, userState, cancellationToken);
            }
        }
        private async Task HandleMediaMessageAsync(Message message, long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var fileId = message.Type switch
            {
                MessageType.Document => message.Document.FileId,
                MessageType.Photo => message.Photo[^1].FileId, // Берем последнюю, т.е. самую высокую по качеству
                MessageType.Video => message.Video.FileId,
                MessageType.Animation => message.Animation.FileId,
                _ => throw new ArgumentOutOfRangeException("неверный тип файла")
            };

            var file = await _botClient.GetFileAsync(fileId, cancellationToken);
            var filePath = $"downloads/{file.FileId}.{file.FilePath.Split('.').Last()}";
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var saveStream = new FileStream(filePath, FileMode.Create))
            {
                await _botClient.DownloadFileAsync(file.FilePath, saveStream, cancellationToken);
            }

            userState.FilePath = filePath;
            await SendTextPromptWithSkipBackAsync(userState.UserId, "Введите продолжительность видео (в секундах) или оставьте пустым", CreateStickerSteps.SetDuration, null, cancellationToken);
        }
    }
}

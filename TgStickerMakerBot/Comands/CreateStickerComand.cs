using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
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

        public async Task DoWorkAsync(long userId, string messageText, UserState userState, CancellationToken cancellationToken)
        {
            switch ((CreateStickerSteps)userState.CurrentStep)
            {
                case CreateStickerSteps.LoadMedia:
                    {
                        if ((IsLink(messageText)) && Uri.TryCreate(messageText, UriKind.Absolute, out _))
                        {
                            await _botClient.SendTextMessageAsync(userId, "Ссылка определена, идет загрузка файла...", cancellationToken: cancellationToken);
                            userState.FilePath = await MediaLoader.LoadMediaFrom(messageText);
                            userState.CurrentStep++;
                            await SendTextPromptWithSkipBackAsync(userId, "Введите продолжительность видео (в секундах) или оставьте пустым", CreateStickerSteps.SetDuration, null, cancellationToken);
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
                        SendTextPromptWithSkipBackAsync(userId, "Введите текст сверху (или оставьте пустым)", CreateStickerSteps.SetTopText, CreateStickerSteps.LoadMedia, cancellationToken);
                        
                        break;
                    }
                case CreateStickerSteps.SetTopText:
                    {
                        userState.CurrentStep++;
                        userState.TopText = messageText;
                        await SendTextPromptWithSkipBackAsync(userId, "Введите текст снизу (или оставьте пустым)", CreateStickerSteps.SetBottomText, CreateStickerSteps.SetTopText, cancellationToken);
                        
                        break;
                    }
                case CreateStickerSteps.SetBottomText:
                    {
                        userState.TopText = messageText;
                        userState.CurrentStep = 0;
                        await SendTextPromptWithSkipBackAsync(userId, "Ожидайте завершения обработки...", null, CreateStickerSteps.SetTopText, cancellationToken);
                        var outputPath = await StickerMaker.ProcessFileAsync(userState.FilePath, userState.VideoDuration, userState.TopText, userState.BottomText, TgStickerMaker.ServiceConfiguration.Settings.BotSettings.OutputDirectory);
                        using (var stream = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var fileName = Path.GetFileName(outputPath);
                            await _botClient.SendDocumentAsync(userId, new InputFileStream(stream, fileName), cancellationToken: cancellationToken);
                        }
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
                        await _botClient.SendTextMessageAsync(userId, "Отправьте файл или ссылку на файл.", cancellationToken: cancellationToken);
                        break;
                    case CreateStickerSteps.SetDuration:
                        await SendTopTextPromptAsync(userId, cancellationToken);
                        break;
                    case CreateStickerSteps.SetTopText:
                        userState.CurrentStep = (int)CreateStickerSteps.SetTopText;
                        await SendBottomTextPromptAsync(userId, cancellationToken);
                        break;
                    case CreateStickerSteps.SetBottomText:
                    {
                        var outputPath = await StickerMaker.ProcessFileAsync(userState.FilePath, userState.VideoDuration, userState.TopText, userState.BottomText, TgStickerMaker.ServiceConfiguration.Settings.BotSettings.OutputDirectory);
                        using (var stream = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var fileName = Path.GetFileName(outputPath);
                            await _botClient.SendDocumentAsync(userId, new InputFileStream(stream, fileName), cancellationToken: cancellationToken);
                        }
                        userState.CurrentStep = (int)CreateStickerSteps.LoadMedia;
                        break;
                    }
                }
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
        }

        private async Task SendBottomTextPromptAsync(long userId, CancellationToken cancellationToken)
        {
            await SendTextPromptWithSkipBackAsync(userId, "Введите текст сверху (или оставьте пустым)", CreateStickerSteps.SetTopText, CreateStickerSteps.LoadMedia, cancellationToken);
        }

        private async Task SendTopTextPromptAsync(long userId, CancellationToken cancellationToken)
        {
            await SendTextPromptWithSkipBackAsync(userId, "Введите текст снизу (или оставьте пустым)", CreateStickerSteps.SetTopText, CreateStickerSteps.LoadMedia, cancellationToken);
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
    }
}

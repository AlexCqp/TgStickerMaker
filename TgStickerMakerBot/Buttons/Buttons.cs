using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using TgStickerMakerBot.Models.CallbackModels;

namespace TgStickerMakerBot.Buttons
{
    public static class Buttons
    {
        public const string SkipComand = "/skip";
        public const string BackComand = "/back";

        public static InlineKeyboardMarkup MainMenu = new InlineKeyboardMarkup(new []
        {
            InlineKeyboardButton.WithCallbackData("1. Создать стикер", JsonConvert.SerializeObject(new CommandCallbackModel(){ CommandClass = Comands.CommandClass.CreateSticker, CurrentStep = 0 })),
            InlineKeyboardButton.WithCallbackData("2. Добавить стикерпак", "/addStickerPack")
        });

        public static InlineKeyboardMarkup SkipBackStep = new InlineKeyboardMarkup(new []
        {
            InlineKeyboardButton.WithCallbackData("Пропустить"),
            InlineKeyboardButton.WithCallbackData("Назад")
        });

        public static InlineKeyboardMarkup GetSkipBackStepButtonsWithCallbackData(CommandCallbackModel skipCommandCallbackModel, CommandCallbackModel backCommandCallbackModel)
        {
            var skipCommandJsonCallback = JsonConvert.SerializeObject(skipCommandCallbackModel);
            var backCommandJsonCallback = JsonConvert.SerializeObject(backCommandCallbackModel);

            return new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Пропустить", skipCommandJsonCallback),
                //InlineKeyboardButton.WithCallbackData("Назад",  backCommandJsonCallback)
            });
        }
    }
}

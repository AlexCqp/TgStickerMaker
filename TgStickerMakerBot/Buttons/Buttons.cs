using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgStickerMakerBot.Buttons
{
    public static class Buttons
    {
        public static InlineKeyboardMarkup[] MainMenu = new InlineKeyboardMarkup[]
        {
            InlineKeyboardButton.WithCallbackData("1. Создать стикер", "/createSticker"),
            InlineKeyboardButton.WithCallbackData("2. Добавить стикерпак", "/addStickerPack")
        };
    }
}

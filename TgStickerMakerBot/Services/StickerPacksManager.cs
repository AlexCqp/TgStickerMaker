using OpenCvSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgStickerMakerBot.Models;
using TgStickerMakerBot.Services.Interfaces;

namespace TgStickerMakerBot.Services
{
    public class StickerPacksManager : IStickerPacksManager
    {
        private readonly ITelegramBotClient _botClient;

        public StickerPacksManager(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task AddStickerToStickerPack(AddStickerModel stickerInfo, Stream sticker)
        {
            var stickerFile = new InputFileStream(sticker); // Стикер-файл
            var inputSticker = new InputSticker(stickerFile, [stickerInfo.Emoji]);
            await _botClient.AddStickerToSetAsync(stickerInfo.UserId, stickerInfo.StickerSetName, inputSticker);
        }

        public async Task CreateStickerPackAsync(AddStickerModel stickerInfo, Stream sticker)
        {
            var userId = stickerInfo.UserId; // ID пользователя, создающего стикерпак
            var stickerPackName = stickerInfo.StickerSetName; // Название стикерпака
            var stickerPackTitle = stickerInfo.StickerSetTitle; // Заголовок стикерпака
            var stickerFile = new InputFileStream(sticker); // Стикер-файл
            var inputSticker = new InputSticker(stickerFile, [stickerInfo.Emoji]);
            await _botClient.CreateNewStickerSetAsync(userId, stickerPackName, stickerPackTitle, [inputSticker], Telegram.Bot.Types.Enums.StickerFormat.Video);
        }

        //public async Task CreateStickerPackAsync(AddStickerModel addStickerModel)
        //{

        //}=

    }
}

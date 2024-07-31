using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgStickerMakerBot.Models;

namespace TgStickerMakerBot.Services.Interfaces
{
    public interface IStickerPacksManager
    {
        public Task CreateStickerPackAsync(AddStickerModel addStickerModel, Stream sticker);

        public Task AddStickerToStickerPack(AddStickerModel stickerInfo, Stream sticker);
    }
}

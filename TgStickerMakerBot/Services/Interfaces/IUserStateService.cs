using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgStickerMakerBot.Services.Interfaces
{
    internal interface IStickerPacksManager
    {
        public void CreateStickerPack(long userId);

        public void AddSticker(long userId);
    }
}

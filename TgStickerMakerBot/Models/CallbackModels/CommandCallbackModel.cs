using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgStickerMakerBot.Comands;

namespace TgStickerMakerBot.Models.CallbackModels
{
    public class CommandCallbackModel
    {
        public CommandClass CommandClass { get; set; }

        public int? CurrentStep { get; set; }
    }
}

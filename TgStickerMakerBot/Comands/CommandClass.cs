using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TgStickerMakerBot.Comands
{
    public enum CommandClass
    {
        None,
        [Display(Name = "createsticker", Description = "Обрабатывает вложение таким образом, что бы оно соответствовало формату стикера для тг")]
        CreateSticker,

        [Display(Name = "сreateandaddstickertoset", Description = "Создает стикер из вложения и добавлеяет его в указанный набор")]
        CreateAndAddStickerToSet,

        [Display(Name = "сreatestickerset", Description = "Создает набор стикеров")]
        CreateStickerSet
    }
}

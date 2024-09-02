namespace TgStickerMakerBot.Models
{
    public class AddStickerModel
    {
        public long UserId { get; set; }

        public string StickerSetName { get; set; }

        public string StickerSetTitle { get; set; }

        public string StickerFilePath { get; set; }

        public string Emoji { get; set; }
    }
}

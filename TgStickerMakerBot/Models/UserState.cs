namespace TgStickerMakerBot.Models
{
    public class UserState
    {
        public UserState(long userId)
        {
            UserId = userId;
        }

        public long UserId { get; set; }

        public int CurrentStep { get; set; } = 0;

        public string FilePath { get; set; }

        public int VideoDuration { get; set; }

        public string TopText { get; set; }

        public string BottomText { get; set; }
    }
}

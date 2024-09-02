using TgStickerMakerBot.Comands;

namespace TgStickerMakerBot.Models
{
    public class UserState
    {
        public UserState(long userId)
        {
            UserId = userId;
        }

        public long UserId { get; set; }

        public CommandClass CurrentCommandClass { get; set; }

        public int CurrentStep { get; set; } = 0;

        public int VideoDuration { get; set; }

        public string TopText { get; set; }

        public string BottomText { get; set; }

        public string FilePath { get; set; }

        public void ClearState()
        {
            CurrentStep = VideoDuration = 0;
            FilePath = TopText = BottomText = null;
            CurrentCommandClass = CommandClass.None;
        }
    }
}

namespace TgStickerMaker.Settings
{
    public class AppSettings
    {
        public string BaseDirectory { get; set; }

        public string LogFilePath { get; set; }

        public string OutputDirectory { get; set; }

        public string MediaDownloadDirectory { get; set; }

        public string? TempFiltes { get; set; }

        public string UpscalerAppPath { get; set; }

        public string PathToFont { get; set; }

        public string FFmpegPath { get; set; }

        public string SecretsPath { get; set; }

        public BotSettings BotSettings { get; set; }
    }
}

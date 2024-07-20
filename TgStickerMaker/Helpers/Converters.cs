using System.Diagnostics;

namespace TgStickerMaker.Helpers
{
    public static class Converters
    {
        public static void ConvertGifToMp4(string gifFilePath, string mp4FilePath)
        {
            var ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"); // Предполагается, что FFmpeg добавлен в PATH
            var arguments = $"-i \"{gifFilePath}\" -movflags faststart -pix_fmt yuv420p -vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \"{mp4FilePath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var error = process.StandardError.ReadToEnd();
            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg error: {error}");
            }
        }
    }
}

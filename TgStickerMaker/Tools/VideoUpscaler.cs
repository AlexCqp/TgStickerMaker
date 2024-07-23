using System.Diagnostics;
using System.Text;

namespace TgStickerMaker.Tools
{
    public static class VideoUpscaler
    {
        public static async Task<string> UpscaleVideoAsync(string inputPath, string outputPath)
        {
            var batFilePath = @"C:\Users\Александр\source\repos\TgStickerMaker\TgStickerMaker\bin\Debug\net8.0\upscale.bat"; // Путь к батнику
            var batDirectory = Path.GetDirectoryName(batFilePath); // Директория, где находится батник

            // Проверка путей
            Console.WriteLine($"Batch File Path: {batFilePath}");
            Console.WriteLine($"Batch Directory: {batDirectory}");
            Console.WriteLine($"Input Path: {inputPath}");
            Console.WriteLine($"Output Path: {outputPath}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c chcp 65001 && \"{batFilePath}\" \"{inputPath}\" \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                StandardInputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = batDirectory // Устанавливаем рабочую директорию
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) => Console.WriteLine($"Output: {e.Data}");
                process.ErrorDataReceived += (sender, e) => Console.WriteLine($"Error: {e.Data}");

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                Console.WriteLine($"Process Exit Code: {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    var errorLogPath = Path.Combine(batDirectory, "upscale_error.log");
                    var errorLog = File.ReadAllText(errorLogPath);
                    throw new Exception($"Error running upscale.bat: Process exited with code {process.ExitCode}. Error Log: {errorLog}");
                }

                return outputPath;
            }
        }


    }
}
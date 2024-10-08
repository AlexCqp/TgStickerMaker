﻿using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TgStickerMaker.Helpers;
using Xabe.FFmpeg;

namespace TgStickerMaker
{
    public static class StickerMaker
    {
        const double DefaultDuration = 3;
        const double DefaultTextSize = 100;
        const double Default40FontSizeWidthInPixel = 23;
        const double DefaultVideoHeight = 512;
        const double DefaultVideoWidth = 512;

        private static readonly Process _process;
        public static async Task<string> ProcessFileAsync(string filePath, double duration, string topText, string bottomText, string outputDirectory)
        {
            if (!Directory.Exists(ServiceConfiguration.Settings.OutputDirectory))
            {
                Directory.CreateDirectory(ServiceConfiguration.Settings.OutputDirectory);
            }

            if (StickerMaker.IsImage(filePath))
            {
                return await ProcessImage(filePath, topText, bottomText, outputDirectory);
            }
            else
            {
                return await ProcessVideo(filePath, topText, bottomText, duration,  outputDirectory);
            }
        }

        public static async Task<string> ProcessImage(string filePath, string topText, string bottomText, string outputDirectory)
        {
            var imagesPath = Path.Combine(outputDirectory, "images");
            if (!Path.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            using var image = await Image.LoadAsync<Rgba32>(filePath);
            ResizeAndAddText(image, topText, bottomText);
            var outputFilePath = WriteFilesHelper.GetUniqueFileName(Path.Combine(imagesPath, "output.png"));
            await image.SaveAsync(outputFilePath);
            return outputFilePath;
        }

        public static async Task<string> ProcessVideo(string filePath, string topText, string bottomText, double duration, string outputDirectory)
        {
            string videoInfo = GetVideoInfo(filePath);
            var sourceVideoDuration = GetVideoDuration(videoInfo);

            var speedUpKoaf = duration == 0 && sourceVideoDuration > 3 ? (DefaultDuration - 0.5) / sourceVideoDuration : 1;
            duration = duration == 0 ? DefaultDuration : duration;
            var videosPath = Path.Combine(outputDirectory, "videos");
            if (!Path.Exists(videosPath))
            {
                Directory.CreateDirectory(videosPath);
            }

            var scaledVideoOutputFile = WriteFilesHelper.GetUniqueFileName(Path.Combine(videosPath, $"output_scaled_step_1_{Path.GetFileName(filePath)}"));
            await ChangeVideoScale(filePath, scaledVideoOutputFile);
            
            string fontPath = ServiceConfiguration.Settings.PathToFont;
            var textFilter = "";

            if (!string.IsNullOrEmpty(topText))
            {
                var font = GetFontSize(topText, DefaultVideoWidth);
                textFilter += $"drawtext=fontfile='{fontPath}':text='{topText}':x=(w-text_w)/2:y=10:fontsize={font}:fontcolor=white:borderw=1:bordercolor=black,";
            }

            if (!string.IsNullOrEmpty(bottomText))
            {
                var font = GetFontSize(bottomText, DefaultVideoWidth);
                textFilter += $"drawtext=fontfile='{fontPath}':text='{bottomText}':x=(w-text_w)/2:y=h-text_h-10:fontsize={font}:fontcolor=white:borderw=1:bordercolor=black,";
            }

            var outputFilePath = WriteFilesHelper.GetUniqueFileName(Path.Combine(videosPath, $"{Path.GetFileName(filePath)}_output.webm"));
            var filterGraph = $"{textFilter}setsar=1,setpts={speedUpKoaf.ToString().Replace(",", ".")}*PTS";
            using (var process = InitProcess(scaledVideoOutputFile, filterGraph,  $"-vcodec libvpx-vp9 -b:v 250k -r 30 {(speedUpKoaf != 1 ? "" : $"-t {duration}")}", outputFilePath))
            {
                process.Start();
                string output = process.StandardError.ReadToEnd();
                await process.WaitForExitAsync();
            }

            return outputFilePath;
        }

        private static async Task ChangeVideoScale(string filePath, string outputFilePath, string resolution = "512x512")
        {
            string filterGraph = $"scale={resolution}";

            using (var process = InitProcess(filePath, filterGraph, " -b:v 250k -r 30", outputFilePath))
            {
                process.Start();
                string output = process.StandardError.ReadToEnd();
                await process.WaitForExitAsync();
            }
        }

        private static Process InitProcess(string filePath, string filterGraph, string filters, string outputFilePath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ServiceConfiguration.Settings.FFmpegPath,
                    Arguments = $"-i \"{filePath}\" -vf \"{filterGraph}\" {filters} \"{outputFilePath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            return process;
        }

        public static async Task<string> ProcessVideo(string filePath, TextOverlay[] textOverlays, double duration)
        {
            var sourceVideoDuration = GetVideoDuration(filePath);
            var speedUpKoaf = sourceVideoDuration > 3 ? (DefaultDuration - 0.5) / sourceVideoDuration : 1;
            duration = duration == 0 ? DefaultDuration : duration;
            var videosPath = Path.Combine(ServiceConfiguration.Settings.OutputDirectory, "videos");
            if (!Directory.Exists(videosPath))
            {
                Directory.CreateDirectory(videosPath);
            }

            string ffmpegPath = ServiceConfiguration.Settings.FFmpegPath;
            Console.WriteLine(ffmpegPath);
            FFmpeg.SetExecutablesPath(ffmpegPath);

            var outputFilePath = WriteFilesHelper.GetUniqueFileName(Path.Combine(videosPath, "output.webm"));
            string fontPath = @"/Windows/Fonts/impact.ttf";

            var textFilter = "";

            foreach (var overlay in textOverlays)
            {
                textFilter += $"drawtext=fontfile='{fontPath}':text='{overlay.Text}':x={overlay.X.ToString().Replace(",",".")}:y={overlay.Y.ToString().Replace(",", ".")}:fontsize={overlay.FontSize}:fontcolor=white:borderw=1:bordercolor=black,";
            }

            if (textFilter.EndsWith(","))
            {
                textFilter = textFilter.Substring(0, textFilter.Length - 1); // Remove trailing comma
            }

            var filterGraph = $"scale=512:512,{textFilter},setsar=1,setpts={speedUpKoaf.ToString().Replace(",", ".")}*PTS";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{filePath}\" -vf \"{filterGraph}\" -vcodec libvpx-vp9 -b:v 250k -r 30 {(speedUpKoaf != 1 ? "" : $"-t {duration}")} \"{outputFilePath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardError.ReadToEnd();
            await process.WaitForExitAsync();
            Console.WriteLine($"{output}");

            return outputFilePath;
        }

        private static int GetFontSize(string text, double videoW)
        {
            var fontSize = Convert.ToInt32(videoW / text.Count() * (37d / 23d));
            return fontSize > 60 ? 60 : fontSize;
        }

        public static bool IsImage(string filePath)
        {
            var extensions = new[] { ".jpeg", ".jpg", ".png" };
            return Array.Exists(extensions, ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        public static (int Width, int Height) ExtractVideoDimensions(string ffmpegOutput)
        {
            // Регулярное выражение для поиска размеров видео
            var regex = new Regex(@"Video:.*?(\d+)x(\d+)");
            var match = regex.Match(ffmpegOutput);

            if (match.Success)
            {
                int width = int.Parse(match.Groups[1].Value);
                int height = int.Parse(match.Groups[2].Value);

                return (width, height);
            }
            else
                regex = new Regex(@"(?<=\s)(\d+)x(\d+)(?=\s)");
                match = regex.Match(ffmpegOutput);

                if (match.Success)
                {
                    int width = int.Parse(match.Groups[1].Value);
                    int height = int.Parse(match.Groups[2].Value);

                    return (width, height);
                }
            
            throw new Exception("Не удалось извлечь размеры видео из вывода FFmpeg.");
            
        }

        public static double GetVideoDuration(string output)
        {
            var match = Regex.Match(output, @"Duration: (\d+):(\d+):(\d+)\.(\d+)");
            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);
                int milliseconds = int.Parse(match.Groups[4].Value);
                return hours * 3600 + minutes * 60 + seconds + milliseconds / 1000.0;
            }

            throw new InvalidOperationException("Could not determine video duration.");
        }

        private static string GetVideoInfo(string filePath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{filePath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private static void ResizeAndAddText(Image<Rgba32> image, string topText, string bottomText)
        {
            image.Mutate(x => x.Resize(512, 512));

            var font = SystemFonts.CreateFont("Arial", 24);
            var options = new TextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };

            if (!string.IsNullOrEmpty(topText))
            {
                image.Mutate(x => x.DrawText(topText, font, Color.White, new PointF(256, 10)));
            }

            if (!string.IsNullOrEmpty(bottomText))
            {
                options.VerticalAlignment = VerticalAlignment.Bottom;
                image.Mutate(x => x.DrawText(bottomText, font, Color.White, new PointF(256, 502)));
            }
        }
    }
}

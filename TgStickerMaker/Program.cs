using SharpHook;
using SharpHook.Native;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using TgStickerMaker;
using TgStickerMaker.Logger;
using TgStickerMaker.MediaLoading;
using static TgStickerMaker.ServiceConfiguration;

SimpleGlobalHook hook;

ConfigureServices();

await Main();
async Task Main()
{
    hook = new SimpleGlobalHook();
    hook.KeyPressed += OnKeyPressed;
    Task.Run(async() => await hook.RunAsync());
    while (true)
    {
        try
        {
            Console.WriteLine("Загрузи файл (.gif, .jpeg, .jpg, .png, .mp4, .webm):");
            var filePath = Console.ReadLine();
            if ((filePath.Contains("http://") || filePath.Contains("https://")) && Uri.TryCreate(filePath, UriKind.Absolute, out var result))
            {
                Console.WriteLine("Ссылка определена, идет загрузка файла");
                filePath = await MediaLoader.LoadMediaFrom(filePath);
            }

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("Файл не найден.");
                continue;
            }

            Console.WriteLine("Введите продолжительность видео (в секундах, или оставьте пустым для автоматического определения или картинки):");
            if (!int.TryParse(Console.ReadLine(), out int duration))
            {
                duration = 0;
            }

            Console.WriteLine("Введите текст сверху (или оставьте пустым):");
            var topText = Console.ReadLine();

            Console.WriteLine("Введите текст снизу (или оставьте пустым):");
            var bottomText = Console.ReadLine();

            string outputFilePath;
            if (StickerMaker.IsImage(filePath))
            {
                outputFilePath = await StickerMaker.ProcessImage(filePath, topText, bottomText);
            }
            else
            {
                outputFilePath = await StickerMaker.ProcessVideo(filePath, topText, bottomText, duration);
            }

            Console.WriteLine($"Файл сохранён как {outputFilePath}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Произошла ошибка при обработке", ex);
        }
    }
    static void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode == KeyCode.VcF3 && (ModifierMask.LeftCtrl) != 0)
        {
            Process.Start("explorer.exe", Settings.OutputDirectory); // Замените на ваш путь
        }
    }
}


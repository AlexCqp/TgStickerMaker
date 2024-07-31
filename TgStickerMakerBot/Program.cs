using TgStickerMakerBot.Services;
using System.ComponentModel.Design;
using TgStickerMakerBot;
using Microsoft.Extensions.DependencyInjection;

await Main();

async Task Main()
{
    try
    {
        ServiceConfiguration.ConfigureServices();
        var settings = TgStickerMaker.ServiceConfiguration.Settings;
        Console.WriteLine($"Bot Token: {settings.BotSettings.BotToken}");

        // Получаем BotService через DI
        var botService = ServiceConfiguration.ServiceProvider.GetRequiredService<BotService>();

        var cts = new CancellationTokenSource();

        // Настраиваем обработчики сигналов завершения
        AppDomain.CurrentDomain.ProcessExit += (s, e) => cts.Cancel();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true; // Предотвращаем завершение процесса
            cts.Cancel();
        };

        await botService.StartAsync(cts.Token);

        // Ожидаем до срабатывания токена отмены
        await Task.Delay(Timeout.Infinite, cts.Token);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
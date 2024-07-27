using static TgStickerMaker.ServiceConfiguration;
using TgStickerMakerBot.Services;

ConfigureServices();
await Main();

async Task Main()
{
    try
    {
        ConfigureServices();
        var botService = new BotService(Settings.BotSettings.BotToken);

        var cts = new CancellationTokenSource();

        // Setup a task to listen for termination signals (e.g., SIGTERM)
        AppDomain.CurrentDomain.ProcessExit += (s, e) => cts.Cancel();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true; // Prevent the process from terminating.
            cts.Cancel();
        };

        await botService.StartAsync(cts.Token);

        // Wait until the cancellation token is triggered
        await Task.Delay(Timeout.Infinite, cts.Token);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
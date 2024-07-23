using static TgStickerMaker.ServiceConfiguration;
using TgStickerMakerBot.Services;

ConfigureServices();
await Main();

async Task Main()
{
    ConfigureServices();
    var botService = new BotService(Settings.BotSettings.BotToken);

    var cts = new CancellationTokenSource();
    await botService.StartAsync(cts.Token);

    Console.WriteLine("Press any key to exit");
    Console.ReadKey();
    cts.Cancel();
}
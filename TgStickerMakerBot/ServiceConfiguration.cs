using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TgStickerMakerBot.Comands;
using TgStickerMakerBot.Services;
using TgStickerMakerBot.Services.Interfaces;

namespace TgStickerMakerBot
{
    public static class ServiceConfiguration
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public static void ConfigureServices()
        {
            TgStickerMaker.ServiceConfiguration.ConfigureServices();
            var services = new ServiceCollection();

            services.AddSingleton<ITelegramBotClient>(provider =>
                new TelegramBotClient(TgStickerMaker.ServiceConfiguration.Settings.BotSettings.BotToken))
                .AddSingleton<BotService>()
                .AddSingleton<IStickerPacksManager, StickerPacksManager>()
                .AddTransient<CreateStickerComand>();
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

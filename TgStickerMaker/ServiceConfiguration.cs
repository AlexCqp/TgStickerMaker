using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TgStickerMaker
{
    public static class ServiceConfiguration
    {
        public static AppSettings Settings;

        public static void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Settings = configuration.GetSection("AppSettings").Get<AppSettings>();
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;
using System.Reflection;
using TgStickerMaker.Settings;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace TgStickerMaker
{
    public static class ServiceConfiguration
    {
        public static AppSettings Settings;
        private const string _botTokenPath = "bot_token";

        public static void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Settings = configuration.GetSection("AppSettings").Get<AppSettings>();
            Console.WriteLine($"{Settings.BotSettings.BotToken}");
            Console.WriteLine(Settings.FFmpegPath);

            InitializeSettingsPaths();

            if (!Path.Exists(Settings.TempFiltes))
            {
                Directory.CreateDirectory(Settings.TempFiltes);
            }

            if (!Path.Exists(Settings.PathToFont))
            {
                Directory.CreateDirectory(Settings.PathToFont);
            }

            if (!Path.Exists(Settings.BotSettings.OutputDirectory))
            {
                Directory.CreateDirectory(Settings.BotSettings.OutputDirectory);
            }
        }

        private static void InitializeSettingsPaths()
        {
            Settings.BaseDirectory = string.IsNullOrWhiteSpace(Settings.BaseDirectory) ? AppContext.BaseDirectory : Settings.BaseDirectory;
            var properties = Settings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Проверяем, является ли свойство строкой
                if (property.PropertyType == typeof(string))
                {
                    var currentValue = (string)property.GetValue(Settings);
                    if (currentValue != null && currentValue.Contains("{BaseDirectory}"))
                    {
                        var newValue = currentValue.Replace("{BaseDirectory}", Settings.BaseDirectory);
                        property.SetValue(Settings, newValue);
                    }
                }
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using TgStickerMakerBot.Comands;

namespace TgStickerMakerBot.Services.Factories
{
    public class CommandCreatorFactory
    {
        public static IBotCommand GetCommand(CommandClass commandClass, IServiceProvider _serviceProvider)
        {
            switch (commandClass)
            {
                case CommandClass.CreateSticker:
                    return _serviceProvider.GetService<CreateStickerComand>();
                default:
                    return null;
            }
        }
    }
}

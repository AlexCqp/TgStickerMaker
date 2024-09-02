using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Telegram.Bot.Types;
using TgStickerMakerBot.Models;

namespace TgStickerMakerBot.Comands
{
    public class CommandHelper
    {
        public static IEnumerable<BotCommand> GetAllCommandsTextAndDescription()
        {
            CommandClass[] commands = (CommandClass[])Enum.GetValues(typeof(CommandClass));

            // Выводим кастомные имена и описания из Display атрибута
            foreach (CommandClass command in commands)
            {
                DisplayAttribute displayAttribute = command.GetType()
                    .GetField(command.ToString())
                    .GetCustomAttribute<DisplayAttribute>();

                if (displayAttribute != null)
                {
                    yield return new BotCommand() 
                    { 
                        Command = displayAttribute.Name, 
                        Description = displayAttribute.Description 
                    };
                }
            }
        }
    }
}

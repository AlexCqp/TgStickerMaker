using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TgStickerMaker.ServiceConfiguration;

namespace TgStickerMaker.Logger
{
    public class Logger
    {
        public static void Log(LogLevel logLevel, string message, Exception ex = null)
        {
            var log = new StringBuilder();
            log.AppendLine($"Лог от {DateTime.Now}, {logLevel}: {message}");
            if (ex != null) {
                log.AppendLine($"Исключение: {ex.Message}");
                log.AppendLine($"Стэк вызывов: {ex.StackTrace}");
            }

            if (IsLogInFile())
            {
                if (!File.Exists(ServiceConfiguration.Settings.LogFilePath))
                {
                    Console.WriteLine($"Пути для файла лога {Path.Combine(AppContext.BaseDirectory, ServiceConfiguration.Settings.LogFilePath)} не существует. Лог будет выводиться в консоль");
                }
                else
                {
                    using (var tsw = new StreamWriter(ServiceConfiguration.Settings.LogFilePath, true))
                    {
                        tsw.Write(log.ToString());
                        return;
                    }
                }
            }

            Console.WriteLine(log.ToString());
        }

        public static void LogDebug(string message, Exception ex = null)
        {
            Log(LogLevel.Debug, message, ex);
        }

        public static void LogWarn(string message, Exception ex = null)
        {
            Log(LogLevel.Warn, message, ex);
        }

        public static void LogInfo(string message, Exception ex = null)
        {
            Log(LogLevel.Info, message, ex);
        }

        public static void LogError(string message, Exception ex = null)
        {
            Log(LogLevel.Error, message, ex);
        }

        private static bool IsLogInFile()
            => !string.IsNullOrEmpty(ServiceConfiguration.Settings.LogFilePath);
    }
}

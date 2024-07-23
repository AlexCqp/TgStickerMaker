using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgStickerMaker.Helpers;
using static TgStickerMaker.ServiceConfiguration;

namespace TgStickerMaker.MediaLoading
{
    public class TenorMediaLoader : IMediaLoaderService
    {
        public async Task<string> LoadMediaAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Загружаем HTML страницы
                    string pageHtml = await client.GetStringAsync(url);

                    // Парсим HTML с помощью HtmlAgilityPack
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(pageHtml);

                    // Ищем элемент <div> с id="single-gif-container"
                    var gifContainer = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='single-gif-container']");

                    if (gifContainer != null)
                    {
                        // Ищем элемент <img> внутри div
                        var gifNode = gifContainer.SelectSingleNode(".//img[contains(@src, '.gif')]");
                        if (gifNode != null)
                        {
                            string gifUrl = gifNode.GetAttributeValue("src", null);

                            if (!string.IsNullOrEmpty(gifUrl))
                            {
                                // Если URL гифки относительный, преобразуем его в абсолютный
                                Uri baseUri = new Uri(url);
                                Uri gifUri = new Uri(baseUri, gifUrl);

                                // Скачиваем гифку
                                var gifFilePath = ServiceConfiguration.Settings.MediaDownloadDirectory;
                                if(!Uri.TryCreate(gifUrl, UriKind.RelativeOrAbsolute, out var res))
                                {
                                    gifFilePath = WriteFilesHelper.GetUniqueFileName(Path.Combine(gifFilePath, "input.gif"));
                                }
                                else
                                {
                                    gifFilePath = WriteFilesHelper.GetUniqueFileName(Path.Combine(gifFilePath, res.Segments.Last()));
                                }

                                byte[] gifBytes = await client.GetByteArrayAsync(gifUri);
                                await File.WriteAllBytesAsync(gifFilePath, gifBytes);
                                Console.WriteLine("Гифка успешно скачана и сохранена.");

                                return gifFilePath;
                            }
                            else
                            {
                                Console.WriteLine("Не удалось найти URL гифки.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Гифка не найдена внутри элемента <div>.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Элемент <div> с id='single-gif-container' не найден.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Произошла ошибка: {e.Message}");
                }
            }

            return null;
        }
    }
}

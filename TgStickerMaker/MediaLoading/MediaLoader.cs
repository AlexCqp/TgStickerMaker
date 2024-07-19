using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgStickerMaker.MediaLoading
{
    public static class MediaLoader
    {
        private const string TenorDomain = "tenor.com";

        public static async Task<string> LoadMediaFrom(string url)
        {
            var isCorrectUrl = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri);
            if (string.IsNullOrEmpty(url) || !isCorrectUrl)
            {
                Console.WriteLine("Че за хуйню ты ввел, олух");
                return null;
            }

            var siteType = DefineSiteType(uri);
            var mediaLoaderService = MediaLoaderFactory.CreateMediaLoader(siteType);

            return await mediaLoaderService.LoadMediaAsync(url);
        }

        private static SiteType DefineSiteType(Uri uri)
        {
            switch (uri.Host)
            {
                case TenorDomain:
                    {
                        return SiteType.Tenor;
                    }
                default:
                    return SiteType.Unknown;
            }
        }
    }
}

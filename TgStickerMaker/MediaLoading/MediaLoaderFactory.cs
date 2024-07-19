using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgStickerMaker.MediaLoading
{
    public static class MediaLoaderFactory
    {
        public static IMediaLoaderService CreateMediaLoader(SiteType siteType)
        {
            switch (siteType)
            {
                case SiteType.Tenor:
                    return new TenorMediaLoader();
                default:
                    throw new ArgumentOutOfRangeException($"Тип сайта не поддерживается");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgStickerMaker.MediaLoading
{
    public interface IMediaLoaderService
    {
        public Task<string> LoadMediaAsync(string url, string filename = null);
    }
}

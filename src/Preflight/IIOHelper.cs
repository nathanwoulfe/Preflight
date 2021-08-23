#if NET472
using Umbraco.Core.IO;
#else 
using Microsoft.AspNetCore.Hosting;
using System.IO;
#endif

namespace Preflight.IO
{
    public interface IIOHelper
    {
        string ResolveUrl(string url);

        string MapPath(string path);
    }

    public class PreflightIoHelper : IIOHelper
    {
#if NET5_0
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly char[] TrimChars = new[] { '~', '/' };

        public PreflightIoHelper(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
#endif

        public string ResolveUrl(string url) =>
#if NET472
            IOHelper.ResolveUrl(url);
#else
            Path.Combine(_webHostEnvironment.WebRootPath, url);
#endif

        public string MapPath(string path) =>
#if NET472
            IOHelper.MapPath(path);
#else
            Path.Combine(_webHostEnvironment.ContentRootPath, path.TrimStart(TrimChars));
#endif
    }
}
#if NETCOREAPP
using Microsoft.AspNetCore.Hosting;
using System.IO;
#else
using Umbraco.Core.IO;
#endif

namespace Preflight.IO
{
    public interface IIOHelper
    {
        string ResolveUrl(string url);

        string MapPath(string path);
    }

#if NETCOREAPP
    public class PreflightIoHelper : IIOHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly char[] TrimChars = new[] { '~', '/' };

        public PreflightIoHelper(IWebHostEnvironment webHostEnvironment) =>        
            _webHostEnvironment = webHostEnvironment;        

        public string ResolveUrl(string url) =>
            Path.Combine(_webHostEnvironment.WebRootPath, url);

        public string MapPath(string path) =>
            Path.Combine(_webHostEnvironment.ContentRootPath, path.TrimStart(TrimChars));
    }
#else
    public class PreflightIoHelper : IIOHelper
    {
        public string ResolveUrl(string url) =>
            IOHelper.ResolveUrl(url);

        public string MapPath(string path) =>
            IOHelper.MapPath(path);
    }
#endif
}
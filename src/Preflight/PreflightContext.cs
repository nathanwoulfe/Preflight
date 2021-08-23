using System.Diagnostics;

#if NET472
using System.Web;
#else 
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
#endif

namespace Preflight
{
    public static class PreflightContext
    {
#if NET472
        private static HttpContextBase _context;

        /// <summary>
        /// Checks for current context then for cached context from startup
        /// in case we are in a background thread or non-web request
        /// </summary>
        public static HttpContextBase Current
        {
            get
            {
                if (HttpContext.Current != null)
                    return new HttpContextWrapper(HttpContext.Current);

                if (_context != null)
                    return _context;

                return HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current);
            }
        }

        public static string HostUrl => Current.Request.Url.Host.ToString();

        // backing field allows setting in tests
        private static bool _isDebuggingEnabled;
        public static bool IsDebuggingEnabled
        {
            get
            {
                return _isDebuggingEnabled || Current.IsDebuggingEnabled && Debugger.IsAttached;
            }
            private set
            {
                _isDebuggingEnabled = value;
            }
        }

        /// <summary>
        /// Is set on startup
        /// </summary>
        /// <param name="context"></param>
        internal static void Set(HttpContextBase context, bool isDebuggingEnabled = false)
        {
            _context = context;
            IsDebuggingEnabled = isDebuggingEnabled;
        }
#else
        public static HttpContext Current => _httpContextAccessor.HttpContext;

        private static bool _isDebuggingEnabled;
        public static bool IsDebuggingEnabled
        {
            get
            {
                return _isDebuggingEnabled || _hostEnvironment.IsDevelopment() && Debugger.IsAttached; ;
            }
            private set
            {
                _isDebuggingEnabled = value;
            }
        }
        public static string HostUrl => Current.Request.Host.Value;

        private static IHttpContextAccessor _httpContextAccessor;
        private static IWebHostEnvironment _hostEnvironment;

        internal static void Set(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostEnvironment, bool isDebuggingEnabled = false)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostEnvironment = hostEnvironment;
            IsDebuggingEnabled = isDebuggingEnabled;
        }
#endif
    }
}
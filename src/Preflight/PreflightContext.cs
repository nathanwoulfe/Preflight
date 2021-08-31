#if NETCOREAPP 
using Microsoft.AspNetCore.Http;
#else
using System.Web;
#endif

namespace Preflight
{
    public static class PreflightContext
    {
#if NETCOREAPP
        public static HttpContext Current => _httpContextAccessor.HttpContext;
        public static string HostUrl => Current.Request.Host.Value;

        private static IHttpContextAccessor _httpContextAccessor;

        internal static void Set(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
#else
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

        /// <summary>
        /// Is set on startup
        /// </summary>
        /// <param name="context"></param>
        internal static void Set(HttpContextBase context)
        {
            _context = context;
        }
#endif
    }
}
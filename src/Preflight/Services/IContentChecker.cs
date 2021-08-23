using Preflight.Models;
#if NET472
using Umbraco.Core.Models;
#else
using Umbraco.Cms.Core.Models;
#endif

namespace Preflight.Services
{
    public interface IContentChecker
    {
        bool CheckContent(int id, bool fromSave = false);
        bool CheckContent(IContent content, bool fromSave = false);

        /// <summary>
        /// Checks set of property values and returns responses via signalr broadcast
        /// </summary>
        /// <param name="dirtyProperties"></param>
        bool CheckDirty(DirtyProperties dirtyProperties);
    }
}

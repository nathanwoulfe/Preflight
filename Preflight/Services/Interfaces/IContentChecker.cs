using Preflight.Models;
using Umbraco.Core.Models;

namespace Preflight.Services.Interfaces
{
    public interface IContentChecker
    {
        bool CheckContent(IContent content, bool fromSave = false);

        /// <summary>
        /// Checks set of property values and returns responses via signalr broadcast
        /// </summary>
        /// <param name="dirtyProperties"></param>
        bool CheckDirty(DirtyProperties dirtyProperties);
    }
}

using System.Collections.Generic;
using Preflight.Models;
using Umbraco.Core.Models;

namespace Preflight.Services.Interfaces
{
    public interface IContentChecker
    {
        PreflightResponseModel Check(IContent content, bool fromSave = false);

        /// <summary>
        /// Checks set of property values and returns responses via signalr broadcast
        /// </summary>
        /// <param name="properties"></param>
        int CheckDirty(IEnumerable<SimpleProperty> properties);
    }
}

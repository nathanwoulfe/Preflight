using Preflight.Models;
using System.Collections.Generic;

namespace Preflight.Services
{
    public interface IValueParserService
    {
        List<PreflightPropertyResponseModel> ParseNestedContent(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave);
        List<PreflightPropertyResponseModel> ParseGridContent(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave);
        List<PreflightPropertyResponseModel> ParseBlockListContent(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave);
        PreflightPropertyResponseModel ParseStringContent(string propertyName, string propertyValue, string culture, string propertyAlias, int nodeId, bool fromSave);
    }
}

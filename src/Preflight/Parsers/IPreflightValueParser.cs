using Preflight.Models;
using System.Collections.Generic;

namespace Preflight.Parsers
{
    public interface IPreflightValueParser
    {
        List<PreflightPropertyResponseModel> Parse(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave);
    }
}

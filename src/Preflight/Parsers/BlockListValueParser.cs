using Preflight.Models;
using System;
using System.Collections.Generic;

namespace Preflight.Parsers
{
    public class BlockListValueParser : IPreflightValueParser
    {
        public List<PreflightPropertyResponseModel> Parse(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave)
        {
            throw new NotImplementedException();
        }
    }
}

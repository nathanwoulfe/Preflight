using Preflight.Executors;
using Preflight.Models;
using System;
using System.Collections.Generic;

namespace Preflight.Parsers
{
    public class StringValueParser : IPreflightValueParser
    {
        private readonly IPluginExecutor _pluginExecutor;

        public StringValueParser(IPluginExecutor pluginExecutor) => _pluginExecutor = pluginExecutor;        

        public List<PreflightPropertyResponseModel> Parse(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave)
        {
            throw new NotImplementedException();
        }
    }
}

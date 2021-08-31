using Preflight.Executors;
using Preflight.Models;
using System.Collections.Generic;

namespace Preflight.Parsers
{
    public class StringValueParser : IPreflightValueParser
    {
        private readonly IPluginExecutor _pluginExecutor;

        public StringValueParser(IPluginExecutor pluginExecutor) => _pluginExecutor = pluginExecutor;

        /// <summary>
        /// Returns the test result for string values, as a list of one item
        /// to maintain a common interface across all parser types
        /// </summary>
        /// <param name="parserParams"></param>
        /// <returns></returns>
        public List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams) => 
            new List<PreflightPropertyResponseModel> { 
                _pluginExecutor.Execute(parserParams) 
            };
    }
}

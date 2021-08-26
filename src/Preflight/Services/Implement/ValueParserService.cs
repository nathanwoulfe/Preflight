using Preflight.Executors;
using Preflight.Models;
using Preflight.Parsers;
using System;
using System.Collections.Generic;

namespace Preflight.Services.Implement
{
    public class ValueParserService : IValueParserService
    {
        private Func<ParserType, IPreflightValueParser> _parserDelegate;
        private readonly IPluginExecutor _pluginExecutor;

        public ValueParserService(Func<ParserType, IPreflightValueParser> parserDelegate, IPluginExecutor pluginExecutor)
        {
            _parserDelegate = parserDelegate;
            _pluginExecutor = pluginExecutor;
        }
        public List<PreflightPropertyResponseModel> ParseBlockListContent(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave)
        {
            IPreflightValueParser parser = _parserDelegate(ParserType.BlockList);
            return parser.Parse(propertyName, propertyValue, culture, nodeId, fromSave);
        }

        public List<PreflightPropertyResponseModel> ParseGridContent(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave)
        {
            IPreflightValueParser parser = _parserDelegate(ParserType.Grid);
            return parser.Parse(propertyName, propertyValue, culture, nodeId, fromSave);
        }

        public List<PreflightPropertyResponseModel> ParseNestedContent(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave)
        {
            IPreflightValueParser parser = _parserDelegate(ParserType.NestedContent);
            return parser.Parse(propertyName, propertyValue, culture, nodeId, fromSave);
        }

        public PreflightPropertyResponseModel ParseStringContent(string propertyName, string propertyValue, string culture, string propertyAlias, int nodeId, bool fromSave)
        {
            return _pluginExecutor.Execute(propertyName, culture, propertyValue, propertyAlias, nodeId, fromSave);
        }
    }
}

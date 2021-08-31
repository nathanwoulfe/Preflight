using Newtonsoft.Json.Linq;
using Preflight.Executors;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System.Collections.Generic;
#if NETCOREAPP
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
#else
using Umbraco.Core.Models;
using Umbraco.Core.Services;
#endif

namespace Preflight.Parsers
{
    public class NestedContentValueParser : PreflightValueParser, IPreflightValueParser
    {
        private readonly IContentTypeService _contentTypeService;

        private const string _ncAlias = "ncContentTypeAlias";

        public NestedContentValueParser(IContentTypeService contentTypeService, IPluginExecutor pluginExecutor, IMessenger messenger) : base(messenger, pluginExecutor)
        {
            _contentTypeService = contentTypeService;
        }

        public List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams)
        {
            Dictionary<string, IContentType> cache = new Dictionary<string, IContentType>();
            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();

            JArray asJson = JArray.Parse(parserParams.PropertyValue);
            var index = 1;

            foreach (JObject o in asJson)
            {
                var typeAlias = o.Value<string>(_ncAlias);
                var type = cache.ContainsKey(typeAlias) ? cache[typeAlias] : _contentTypeService.Get(typeAlias);

                if (!cache.ContainsKey(typeAlias))
                {
                    cache.Add(typeAlias, type);
                }

                var propsFromType = type.GetPreflightProperties();

                foreach (var property in propsFromType)
                {
                    var propertyValue = o.Value<string>(property.Alias);
                    string label = GetLabel(parserParams.PropertyName, property.Name, o.Value<string>("name"));

                    if (!propertyValue.HasValue())
                    {
                        SendRemoveResponse(property.Name, label);
                    }
                    else
                    {
                        parserParams.PropertyValue = propertyValue;

                        var model = ExecutePlugin(parserParams, label, KnownPropertyAlias.NestedContent);

                        response.Add(model);
                    }
                }

                index += 1;
            }

            return response;
        }
    }
}

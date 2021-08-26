using Newtonsoft.Json.Linq;
using Preflight.Executors;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
#else
using Umbraco.Core.Models;
using Umbraco.Core.Services;
#endif

namespace Preflight.Parsers
{
    public class NestedContentValueParser : IPreflightValueParser
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IPluginExecutor _pluginExecutor;
        private readonly IMessenger _messenger;

        private const string _ncAlias = "ncContentTypeAlias";

        public NestedContentValueParser(IContentTypeService contentTypeService, IPluginExecutor pluginExecutor, IMessenger messenger)
        {
            _contentTypeService = contentTypeService;
            _pluginExecutor = pluginExecutor;
            _messenger = messenger;
        }

        public List<PreflightPropertyResponseModel> Parse(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave)
        {
            Dictionary<string, IContentType> cache = new Dictionary<string, IContentType>();
            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();

            JArray asJson = JArray.Parse(propertyValue);
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
                    var value = o.Value<string>(property.Alias);
                    string label = $"{propertyName} (Item {index} - {property.Name})";

                    if (!value.HasValue())
                    {
                        _messenger.SendTestResult(new PreflightPropertyResponseModel
                        {
                            Name = property.Name,
                            Label = label,
                            Remove = true
                        });
                    }
                    else
                    {
                        PreflightPropertyResponseModel model = _pluginExecutor.Execute(propertyName, culture, value, property.PropertyEditorAlias, nodeId, fromSave, KnownPropertyAlias.NestedContent);

                        model.Label = label;
                        model.TotalTests = model.Plugins.Aggregate(0, (acc, x) => acc + x.TotalTests);

                        response.Add(model);
                    }
                }

                index += 1;
            }

            return response;
        }
    }
}

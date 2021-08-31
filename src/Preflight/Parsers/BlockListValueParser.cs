using Newtonsoft.Json;
using Preflight.Executors;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.Blocks;
#else
using Umbraco.Core.Models;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Services;
#endif

namespace Preflight.Parsers
{
    public class BlockListValueParser : PreflightValueParser, IPreflightValueParser
    {
        private readonly IContentTypeService _contentTypeService;

        public BlockListValueParser(IContentTypeService contentTypeService, IPluginExecutor pluginExecutor, IMessenger messenger) : base(messenger, pluginExecutor)
        {
            _contentTypeService = contentTypeService;
        }

        public List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams)
        {
            Dictionary<Guid, IContentType> cache = new Dictionary<Guid, IContentType>();
            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();

            var blockValue = JsonConvert.DeserializeObject<BlockValue>(parserParams.PropertyValue);
            var index = 1;

            foreach (var blockItem in blockValue.ContentData)
            {
                var typeAlias = blockItem.ContentTypeKey;
                var type = cache.ContainsKey(typeAlias) ? cache[typeAlias] : _contentTypeService.Get(typeAlias);

                if (!cache.ContainsKey(typeAlias))
                {
                    cache.Add(typeAlias, type);
                }

                var propsFromType = type.GetPreflightProperties();

                foreach (var property in propsFromType)
                {
                    var propertyValue = blockItem.RawPropertyValues.FirstOrDefault(p => p.Key == property.Alias).Value?.ToString();
                    string label = GetLabel(parserParams.PropertyName, property.Name, index: index);

                    if (!propertyValue.HasValue())
                    {
                        SendRemoveResponse(property.Name, label);
                    }
                    else
                    {
                        parserParams.PropertyValue = propertyValue;
                        var model = ExecutePlugin(parserParams, label, KnownPropertyAlias.BlockList);
                        response.Add(model);
                    }
                }

                index += 1;
            }

            return response;
        }
    }
}

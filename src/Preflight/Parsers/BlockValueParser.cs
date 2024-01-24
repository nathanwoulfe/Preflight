using Newtonsoft.Json;
using Preflight.Executors;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

namespace Preflight.Parsers;

public class BlockValueParser : PreflightValueParserBase, IPreflightValueParser
{
    private readonly IContentTypeService _contentTypeService;

    public BlockValueParser(IContentTypeService contentTypeService, IPluginExecutor pluginExecutor, IMessenger messenger)
        : base(messenger, pluginExecutor)
    {
        _contentTypeService = contentTypeService;
    }

    public List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams)
    {
        Dictionary<Guid, IContentType> cache = [];
        List<PreflightPropertyResponseModel> response = [];

        BlockValue? blockValue = JsonConvert.DeserializeObject<BlockValue>(parserParams.PropertyValue);
        int index = 1;

        if (blockValue is null)
        {
            return response;
        }

        foreach (BlockItemData blockItem in blockValue.ContentData)
        {
            Guid typeAlias = blockItem.ContentTypeKey;
            IContentType? type = cache.TryGetValue(typeAlias, out IContentType? value) ? value : _contentTypeService.Get(typeAlias);

            if (type is null)
            {
                continue;
            }

            _ = cache.TryAdd(typeAlias, type);
            IEnumerable<IPropertyType> propsFromType = type.GetPreflightProperties();

            foreach (IPropertyType property in propsFromType)
            {
                string? propertyValue = blockItem.RawPropertyValues.FirstOrDefault(p => p.Key == property.Alias).Value?.ToString();

                if (propertyValue is null)
                {
                    continue;
                }

                string label = GetLabel(parserParams.PropertyName, property.Name, index: index);

                if (!propertyValue.HasValue())
                {
                    SendRemoveResponse(property.Name, label, parserParams.NodeId);
                }
                else
                {
                    parserParams.PropertyValue = propertyValue;
                    PreflightPropertyResponseModel model = ExecutePlugin(parserParams, label, KnownPropertyAlias.BlockList);
                    response.Add(model);
                }
            }

            index += 1;
        }

        return response;
    }
}

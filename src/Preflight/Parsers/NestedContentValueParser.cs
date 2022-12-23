using Newtonsoft.Json.Linq;
using Preflight.Executors;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Preflight.Parsers;

public class NestedContentValueParser : PreflightValueParserBase, IPreflightValueParser
{
    private readonly IContentTypeService _contentTypeService;

    private const string NcAlias = "ncContentTypeAlias";

    public NestedContentValueParser(IContentTypeService contentTypeService, IPluginExecutor pluginExecutor, IMessenger messenger)
        : base(messenger, pluginExecutor)
    {
        _contentTypeService = contentTypeService;
    }

    public List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams)
    {
        Dictionary<string, IContentType> cache = new();
        List<PreflightPropertyResponseModel> response = new();

        var asJson = JArray.Parse(parserParams.PropertyValue);
        int index = 1;

        foreach (JObject o in asJson)
        {
            string? typeAlias = o.Value<string>(NcAlias);

            if (typeAlias is null)
            {
                continue;
            }

            IContentType? type = cache.ContainsKey(typeAlias) ? cache[typeAlias] : _contentTypeService.Get(typeAlias);

            if (type is null)
            {
                continue;
            }

            if (!cache.ContainsKey(typeAlias))
            {
                cache.Add(typeAlias, type);
            }

            IEnumerable<IPropertyType> propsFromType = type.GetPreflightProperties();

            foreach (IPropertyType property in propsFromType)
            {
                string? propertyValue = o.Value<string>(property.Alias);

                if (propertyValue is null)
                {
                    continue;
                }

                string label = GetLabel(parserParams.PropertyName, property.Name, o.Value<string>("name")!);

                if (!propertyValue.HasValue())
                {
                    SendRemoveResponse(property.Name, label);
                }
                else
                {
                    parserParams.PropertyValue = propertyValue;

                    PreflightPropertyResponseModel model = ExecutePlugin(parserParams, label, KnownPropertyAlias.NestedContent);

                    response.Add(model);
                }
            }

            index += 1;
        }

        return response;
    }
}

using Newtonsoft.Json;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Parsers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

namespace Preflight.Services.Implement;

/// <summary>
/// Where the magic happens. ContentChecker extracts property values and passes them into the set of plugins for testing
/// </summary>
internal sealed class ContentChecker : IContentChecker
{
    private readonly IContentService _contentService;
    private readonly IMessenger _messenger;
    private readonly Func<ParserType, IPreflightValueParser> _parserDelegate;

    public ContentChecker(
        IContentService contentService,
        IMessenger messenger,
        Func<ParserType, IPreflightValueParser> parserDelegate)
    {
        _contentService = contentService;
        _messenger = messenger;
        _parserDelegate = parserDelegate;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dirtyProperties"></param>
    public bool CheckDirty(DirtyProperties dirtyProperties)
    {
        var parserParams = new ContentParserParams
        {
            NodeId = dirtyProperties.Id,
            Culture = dirtyProperties.Culture,
            FromSave = false,
        };

        bool failed = false;

        foreach (SimpleProperty prop in dirtyProperties.Properties)
        {
            // if the prop is blocklist, the value needs to be modified as front-end
            // sends an array of BlockItemData, but backend expects a BlockValue
            // 
            // bit messy, but prevents circular json errors on client side
            if (prop.Editor == KnownPropertyAlias.BlockList && prop.Value.HasValue())
            {
                var blockValue = new BlockValue
                {
                    ContentData = JsonConvert.DeserializeObject<List<BlockItemData>>(prop.Value) ?? new(),
                };

                prop.Value = JsonConvert.SerializeObject(blockValue);
            }

            parserParams.PropertyValue = prop.Value;

            // only continue if the prop has a value
            if (!parserParams.PropertyValue.HasValue())
            {
                _messenger.SendTestResult(new PreflightPropertyResponseModel
                {
                    Name = prop.Name,
                    Remove = true,
                });

                continue;
            }

            parserParams.PropertyName = prop.Name;
            parserParams.PropertyEditorAlias = prop.Editor;

            failed = TestAndBroadcast(parserParams) || failed;
        }

        _messenger.PreflightComplete();

        return failed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="culture"></param>
    /// <param name="fromSave"></param>
    /// <returns></returns>
    public bool CheckContent(int id, string culture, bool fromSave)
    {
        IContent? contentToCheck = _contentService.GetById(id);
        if (contentToCheck == null)
        {
            return false;
        }

        return CheckContent(contentToCheck, culture, fromSave);
    }

    /// <summary>
    /// Checks all testable properties on the given IContent item
    /// </summary>
    /// <param name="content"></param>
    /// <param name="culture"></param>
    /// <param name="fromSave"></param>
    /// <returns></returns>
    public bool CheckContent(IContent content, string culture, bool fromSave)
    {
        var parserParams = new ContentParserParams
        {
            NodeId = content.Id,
            Culture = culture,
            FromSave = fromSave,
        };

        bool failed = false;

        IEnumerable<IProperty> props = content.GetPreflightProperties();

        foreach (IProperty prop in props)
        {
            parserParams.PropertyValue = (prop.GetValue(culture) ?? prop.GetValue())?.ToString() ?? string.Empty;

            // only continue if the prop has a value
            if (!parserParams.PropertyValue.HasValue())
            {
                _messenger.SendTestResult(new PreflightPropertyResponseModel
                {
                    Name = prop.PropertyType.Name,
                    Remove = true,
                });

                continue;
            }

            parserParams.PropertyName = prop.PropertyType.Name;
            parserParams.PropertyEditorAlias = prop.PropertyType.PropertyEditorAlias;

            failed = TestAndBroadcast(parserParams) || failed;
        }

        _messenger.PreflightComplete();

        return failed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parserParams"></param>
    /// <param name="name"></param>
    /// <param name="culture"></param>
    /// <param name="value"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    private bool TestAndBroadcast(ContentParserParams parserParams)
    {
        List<PreflightPropertyResponseModel> testResult = new();

        bool failed = false;
        ParserType? parserType = EnumExtensions.GetByParsablePropertyAlias<ParserType>(parserParams.PropertyEditorAlias);

        if (parserType is null)
        {
            return true;
        }

        testResult = _parserDelegate(parserType.Value).Parse(parserParams);

        // return the results via signalr for perceived perf
        foreach (PreflightPropertyResponseModel result in testResult)
        {
            if (result.Plugins.Count <= 0)
            {
                continue;
            }

            if (result.Failed)
            {
                failed = true;
            }

            // announce the result
            _messenger.SendTestResult(result);
        }

        return failed;
    }
}

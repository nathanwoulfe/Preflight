using Preflight.Executors;
using Preflight.Models;
using Preflight.Services;

namespace Preflight.Parsers;

public class PreflightValueParserBase
{
    private readonly IMessenger _messenger;
    private readonly IPluginExecutor _pluginExecutor;

    public PreflightValueParserBase(IMessenger messenger, IPluginExecutor pluginExecutor)
    {
        _messenger = messenger;
        _pluginExecutor = pluginExecutor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="innerPropertyName"></param>
    /// <param name="rowName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetLabel(string propertyName, string innerPropertyName, string rowName = "", int index = 0) => index > 0 ?
            $"{propertyName} (Item {index} - {innerPropertyName})" :
            $"{propertyName} ({rowName} - {innerPropertyName})";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    public void SendRemoveResponse(string name, string label) => _messenger.SendTestResult(new PreflightPropertyResponseModel
    {
        Name = name,
        Label = label,
        Remove = true,
    });

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parserParams"></param>
    /// <param name="label"></param>
    /// <param name="propertyAlias"></param>
    /// <returns></returns>
    public PreflightPropertyResponseModel ExecutePlugin(ContentParserParams parserParams, string label, string propertyAlias)
    {
        PreflightPropertyResponseModel model = _pluginExecutor.Execute(parserParams, propertyAlias);
        model.Label = label;

        return model;
    }
}

using Newtonsoft.Json;
using Preflight.Models.Settings;
using Preflight.Plugins;

namespace Preflight.Models;

/// <summary>
/// 
/// </summary>
public class PreflightPluginResponseModel
{
    public PreflightPluginResponseModel(IPreflightPlugin src)
    {
        Failed = src.Failed;
        FailedCount = src.FailedCount;
        TotalTests = src.TotalTests;
        SortOrder = src.SortOrder;
        Name = src.Name;
        ViewPath = src.ViewPath;
        Summary = src.Summary;
        Description = src.Description;
        Result = src.Result;
        Settings = src.Settings;

        if (FailedCount == 0)
        {
            FailedCount = Failed ? 1 : 0;
        }
    }

    [JsonProperty("failed")]
    public bool Failed { get; }

    [JsonProperty("failedCount")]
    public int FailedCount { get; }

    [JsonProperty("totalTest")]
    public int TotalTests { get; }

    [JsonProperty("sortOrder")]
    public int SortOrder { get; }

    [JsonProperty("name")]
    public string Name { get; }

    [JsonProperty("viewPath")]
    public string ViewPath { get; }

    [JsonProperty("summary")]
    public string Summary { get; }

    [JsonProperty("description")]
    public string Description { get; }

    [JsonProperty("result")]
    public object? Result { get; }

    [JsonProperty("settings")]
    public IEnumerable<SettingsModel> Settings { get; }
}

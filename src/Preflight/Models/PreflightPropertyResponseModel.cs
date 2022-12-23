using Newtonsoft.Json;
using Preflight.Plugins;

namespace Preflight.Models;

public class PreflightPropertyResponseModel
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("label")]
    public string Label { get; set; } = string.Empty;

    [JsonProperty("plugins")]
    public List<PreflightPluginResponseModel> Plugins { get; set; } = new();

    [JsonProperty("open")]
    public bool Open { get; set; } = false;

    [JsonProperty("failed")]
    public bool Failed { get; set; } = false;

    [JsonProperty("failedCount")]
    public int FailedCount { get; set; } = -1;

    [JsonProperty("totalTests")]
    public int TotalTests { get; set; }

    [JsonProperty("remove")]
    public bool Remove { get; set; } = false;
}

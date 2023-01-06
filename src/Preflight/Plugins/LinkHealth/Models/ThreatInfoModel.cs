using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class ThreatInfoModel
{
    [JsonProperty("threatTypes")]
    public string[] ThreatTypes { get; set; } = { };

    [JsonProperty("platformTypes")]
    public string[] PlatformTypes { get; set; } = { };

    [JsonProperty("threatEntryTypes")]
    public string[] ThreatEntryTypes { get; set; } = { };

    [JsonProperty("threatEntries")]
    public ThreatEntryModel[] ThreatEntries { get; set; } = { };
}


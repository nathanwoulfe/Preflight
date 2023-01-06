using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class MatchModel
{
    [JsonProperty("threatType")]
    public string ThreatType { get; set; } = string.Empty;

    [JsonProperty("platformType")]
    public string PlatformType { get; set; } = string.Empty;

    [JsonProperty("threatEntryType")]
    public string ThreatEntryType { get; set; } = string.Empty;

    [JsonProperty("threat")]
    public ThreatModel Threat { get; set; } = new();

    [JsonProperty("threatEntryMetadata")]
    public ThreatEntryMetadataModel ThreatEntryMetadata { get; set; } = new();

    [JsonProperty("cacheDuration")]
    public string CacheDuration { get; set; } = string.Empty;
}

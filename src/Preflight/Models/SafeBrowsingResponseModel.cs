using Newtonsoft.Json;

namespace Preflight.Models;

public class SafeBrowsingResponseModel
{
    [JsonProperty("matches")]
    public List<Match> Matches { get; set; } = new();
}

public class Match
{
    [JsonProperty("threatType")]
    public string ThreatType { get; set; } = string.Empty;

    [JsonProperty("platformType")]
    public string PlatformType { get; set; } = string.Empty;

    [JsonProperty("threatEntryType")]
    public string ThreatEntryType { get; set; } = string.Empty;

    [JsonProperty("threat")]
    public Threat Threat { get; set; } = new();

    [JsonProperty("threatEntryMetadata")]
    public ThreatEntryMetadata ThreatEntryMetadata { get; set; } = new();

    [JsonProperty("cacheDuration")]
    public string CacheDuration { get; set; } = string.Empty;
}

public class Threat
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
}

public class Entry
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;
}

public class ThreatEntryMetadata
{
    [JsonProperty("entries")]
    public List<Entry> Entries { get; set; } = new();
}

using Newtonsoft.Json;

namespace Preflight.Models;

public class SafeBrowsingRequestModel
{
    [JsonProperty("client")]
    public Client Client { get; set; } = new();

    [JsonProperty("threatInfo")]
    public ThreatInfo ThreatInfo { get; set; } = new();
}

public class Client
{
    [JsonProperty("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonProperty("clientVersion")]
    public string ClientVersion { get; set; } = string.Empty;
}

public class ThreatInfo
{
    [JsonProperty("threatTypes")]
    public string[] ThreatTypes { get; set; } = { };

    [JsonProperty("platformTypes")]
    public string[] PlatformTypes { get; set; } = { };

    [JsonProperty("threatEntryTypes")]
    public string[] ThreatEntryTypes { get; set; } = { };

    [JsonProperty("threatEntries")]
    public ThreatEntry[] ThreatEntries { get; set; } = { };
}

public class ThreatEntry
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
}

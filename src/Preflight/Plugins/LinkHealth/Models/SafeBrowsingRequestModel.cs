using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class SafeBrowsingRequestModel
{
    [JsonProperty("client")]
    public ClientModel Client { get; set; } = new();

    [JsonProperty("threatInfo")]
    public ThreatInfoModel ThreatInfo { get; set; } = new();
}

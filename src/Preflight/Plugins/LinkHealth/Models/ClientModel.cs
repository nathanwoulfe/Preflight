using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class ClientModel
{
    [JsonProperty("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonProperty("clientVersion")]
    public string ClientVersion { get; set; } = string.Empty;
}

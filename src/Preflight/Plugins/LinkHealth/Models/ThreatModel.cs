using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class ThreatModel
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
}

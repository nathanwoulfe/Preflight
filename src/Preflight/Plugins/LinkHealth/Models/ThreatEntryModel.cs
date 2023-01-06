using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class ThreatEntryModel
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
}

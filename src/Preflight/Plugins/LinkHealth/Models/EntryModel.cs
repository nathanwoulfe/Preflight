using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class EntryModel
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;
}

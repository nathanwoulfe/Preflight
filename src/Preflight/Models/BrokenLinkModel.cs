using Newtonsoft.Json;

namespace Preflight.Models;

public class BrokenLinkModel
{
    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;

    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("unsafe")]
    public bool Unsafe { get; set; }
}

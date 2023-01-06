using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class SafeBrowsingResponseModel
{
    [JsonProperty("matches")]
    public List<MatchModel> Matches { get; set; } = new();
}

using Newtonsoft.Json;

namespace Preflight.Models.Settings;

public class SettingsTabModel
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// This must be explicitly set whenever a tab is created, it can't be lazy as it 
    /// would then change when localizing, which we don't wany
    /// </summary>
    [JsonProperty("alias")]
    public string Alias { get; set; } = string.Empty;

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
}

using Newtonsoft.Json;

namespace Preflight.Models.Settings;

public class PreflightSettingsModel
{
    [JsonProperty("settings")]
    public List<SettingsModel> Settings { get; set; } = [];

    [JsonProperty("tabs")]
    public List<SettingsTabModel> Tabs { get; set; } = [];
}

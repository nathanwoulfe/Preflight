using Newtonsoft.Json;

namespace Preflight.Models
{
    public class SettingsModel
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("view")]
        public string View { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("tab")]
        public string Tab { get; set; }
    }
}

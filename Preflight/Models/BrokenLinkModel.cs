using Newtonsoft.Json;

namespace Preflight.Models
{
    public class BrokenLinkModel
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("unsafe")]
        public bool Unsafe { get; set; }
    }
}

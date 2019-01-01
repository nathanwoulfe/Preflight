using Newtonsoft.Json;
using System.Collections.Generic;
using Preflight.Constants;

namespace Preflight.Models
{
    public class PreflightResponseModel
    {
        [JsonProperty("properties")]
        public List<PreflightPropertyResponseModel> Properties { get; set; }

        [JsonProperty("failed")]
        public bool Failed { get; set; }

        [JsonProperty(KnownStrings.CheckLinksAlias)]
        public bool CheckLinks { get; set; }

        [JsonProperty(KnownStrings.CheckReadabilityAlias)]
        public bool CheckReadability { get; set; }

        [JsonProperty(KnownStrings.CheckSafeBrowsingAlias)]
        public bool CheckSafeBrowsing { get; set; }

        public PreflightResponseModel()
        {
            Properties = new List<PreflightPropertyResponseModel>();
            Failed = false;
        }
    }

    public class PreflightPropertyResponseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("links")]
        public List<BrokenLinkModel> Links { get; set; }

        [JsonProperty("safeBrowsing")]
        public List<BrokenLinkModel> SafeBrowsing { get; set; }

        [JsonProperty("readability")]
        public ReadabilityResponseModel Readability { get; set; }

        [JsonProperty("collapsed")]
        public bool Collapsed { get; set; }

        [JsonProperty("failed")]
        public bool Failed { get; set; }

        public PreflightPropertyResponseModel()
        {
            Collapsed = true;
            Failed = false;
        }
    }
}

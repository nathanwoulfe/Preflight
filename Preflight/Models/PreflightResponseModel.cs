using Newtonsoft.Json;
using System.Collections.Generic;
using Preflight.Plugins;

namespace Preflight.Models
{
    public class PreflightResponseModel
    {
        [JsonProperty("properties")]
        public List<PreflightPropertyResponseModel> Properties { get; set; }

        [JsonProperty("failed")]
        public bool Failed { get; set; }

        [JsonProperty("cancelSaveOnFail")]
        public bool CancelSaveOnFail { get; set; }

        [JsonProperty("hideDisabled")]
        public bool HideDisabled { get; set; }

        [JsonProperty("checkLinks")]
        public bool CheckLinks { get; set; }

        [JsonProperty("checkReadability")]
        public bool CheckReadability { get; set; }

        [JsonProperty("checkSafeBrowsing")]
        public bool CheckSafeBrowsing { get; set; }

        [JsonProperty("failedCount")]
        public int FailedCount { get; set; }

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

        [JsonProperty("plugins")]
        public List<PreflightPlugin> Plugins { get; set; }

        [JsonProperty("open")]
        public bool Open { get; set; }

        [JsonProperty("failed")]
        public bool Failed { get; set; }

        [JsonProperty("failedCount")]
        public int FailedCount { get; set; }

        public PreflightPropertyResponseModel()
        {
            Open = false;
            Failed = false;

            Plugins = new List<PreflightPlugin>();
        }
    }
}

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

        [JsonProperty("plugins")]
        public List<IPreflightPlugin> Plugins { get; set; }

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

            Plugins = new List<IPreflightPlugin>();
        }
    }
}

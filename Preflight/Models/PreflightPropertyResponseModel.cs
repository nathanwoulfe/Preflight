using Newtonsoft.Json;
using System.Collections.Generic;
using Preflight.Plugins;

namespace Preflight.Models
{
    public class PreflightPropertyResponseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        internal string Label { get; set; }

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
            FailedCount = -1;

            Plugins = new List<IPreflightPlugin>();
        }
    }
}

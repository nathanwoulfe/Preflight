using System.Collections.Generic;
using Newtonsoft.Json;

namespace Preflight.Models
{
    public class SafeBrowsingRequestModel
    {
        [JsonProperty("client")]
        public Client Client { get; set; }

        [JsonProperty("threatInfo")]
        public ThreatInfo ThreatInfo { get; set; }
    }

    public class Client
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientVersion")]
        public string ClientVersion { get; set; }
    }

    public class ThreatInfo
    {
        [JsonProperty("threatTypes")]
        public string[] ThreatTypes { get; set; }

        [JsonProperty("platformTypes")]
        public string[] PlatformTypes { get; set; }

        [JsonProperty("threatEntryTypes")]
        public string[] ThreatEntryTypes { get; set; }

        [JsonProperty("threatEntries")]
        public ThreatEntry[] ThreatEntries { get; set; }
    }

    public class ThreatEntry
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}

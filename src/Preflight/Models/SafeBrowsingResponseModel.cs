using Newtonsoft.Json;
using System.Collections.Generic;

namespace Preflight.Models
{
    public class SafeBrowsingResponseModel
    {
        [JsonProperty("matches")]
        public List<Match> Matches { get; set; }

        public SafeBrowsingResponseModel()
        {
            Matches = new List<Match>();
        }
    }

    public class Match
    {
        [JsonProperty("threatType")]
        public string ThreatType { get; set; }

        [JsonProperty("platformType")]
        public string PlatformType { get; set; }

        [JsonProperty("threatEntryType")]
        public string ThreatEntryType { get; set; }

        [JsonProperty("threat")]
        public Threat Threat { get; set; }

        [JsonProperty("threatEntryMetadata")]
        public ThreatEntryMetadata ThreatEntryMetadata { get; set; }

        [JsonProperty("cacheDuration")]
        public string CacheDuration { get; set; }
    }

    public class Threat
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Entry
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class ThreatEntryMetadata
    {
        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }
    }


}

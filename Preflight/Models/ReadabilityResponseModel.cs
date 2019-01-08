using Newtonsoft.Json;
using System.Collections.Generic;

namespace Preflight.Models
{
    public class ReadabilityResponseModel
    {
        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("averageSyllables")]
        public double AverageSyllables { get; set; }

        [JsonProperty("sentenceLength")]
        public double SentenceLength { get; set; }

        [JsonProperty("longWords")]
        public List<string> LongWords { get; set; }

        [JsonProperty("blacklist")]
        public List<string> Blacklist { get; set; }

        [JsonProperty("targetMin")]
        public int TargetMin { get; set; }

        [JsonProperty("targetMax")]
        public int TargetMax { get; set; }

        [JsonProperty("longWordSyllables")]
        public int LongWordSyllables { get; set; }

        /// <summary>
        /// Property will fail if readability is outside the range, or blacklisted/long words exist
        /// </summary>
        [JsonProperty("failed")]
        public bool Failed { get; set; }

        [JsonProperty("failedReadability")]
        public bool FailedReadability { get; set; }

        public ReadabilityResponseModel()
        {
            LongWords = new List<string>();
            Failed = false;
        }
    }
}

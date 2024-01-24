using Newtonsoft.Json;

namespace Preflight.Plugins.Readability;

public class ReadabilityResponseModel
{
    [JsonProperty("score")]
    public double Score { get; set; }

    [JsonProperty("averageSyllables")]
    public double AverageSyllables { get; set; }

    [JsonProperty("sentenceLength")]
    public double SentenceLength { get; set; }

    [JsonProperty("longWords")]
    public List<string> LongWords { get; set; } = [];

    [JsonProperty("blacklist")]
    public List<string> Blacklist { get; set; } = [];

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
    public bool Failed { get; set; } = false;

    [JsonProperty("failedReadability")]
    public bool FailedReadability { get; set; } = false;
}

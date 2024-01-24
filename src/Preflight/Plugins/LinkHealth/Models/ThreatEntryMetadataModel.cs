using Newtonsoft.Json;

namespace Preflight.Plugins.LinkHealth.Models;

public class ThreatEntryMetadataModel
{
    [JsonProperty("entries")]
    public List<EntryModel> Entries { get; set; } = [];
}

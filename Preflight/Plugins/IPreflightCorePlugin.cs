using Newtonsoft.Json;

namespace Preflight.Plugins
{
    public interface IPreflightCorePlugin : IPreflightPlugin
    {
        /// <summary>
        /// Is this a core plugin, shipped with with package?
        /// </summary>
        [JsonProperty("core")]
        bool Core { get; }
    }
}

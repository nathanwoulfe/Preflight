using System.Collections.Generic;
using ClientDependency.Core;
using Newtonsoft.Json;
using Preflight.Extensions;

namespace Preflight.Models
{
    public class PreflightSettings
    {
        [JsonProperty("settings")]
        public List<SettingsModel> Settings { get; set; }

        [JsonProperty("tabs")]
        public List<SettingsTab> Tabs { get; set; }
    }

    public class SettingsTab
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("alias")]
        public string Alias => Name.Camel();

        [JsonProperty("open")]
        public bool Open => false;

        public SettingsTab(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// The model describing a generic setting for a test. A test can have none, one or many
    /// </summary>
    public class SettingsModel
    {
        [JsonProperty("core")]
        internal bool Core { get; set; }

        /// <summary>
        /// A UI-friendly label for the setting
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// Set the default value for the setting
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <summary>
        /// Describe the setting
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Path to an Umbraco property editor view eg "views/propertyeditors/boolean/boolean.html"
        /// </summary>
        [JsonProperty("view")]
        public string View { get; set; }

        /// <summary>
        /// Where should the setting sit on the tab
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Where should the setting be displayed - either reference an existing tab from SettingsTabNames, or add your own
        /// Plugins default to the plugin name
        /// </summary>
        [JsonProperty("tab")]
        internal string Tab { get; set; }

        /// <summary>
        /// The generated property alias
        /// </summary>
        [JsonProperty("alias")]
        internal string Alias { get; set; }
    }
}

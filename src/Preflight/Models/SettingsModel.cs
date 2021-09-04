using Newtonsoft.Json;
using Preflight.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

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
        
        /// <summary>
        /// This must be explicitly set whenever a tab is created, it can't be lazy as it 
        /// would then change when localizing, which we don't wany
        /// </summary>
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    /// <summary>
    /// The model describing a generic setting for a test. A test can have none, one or many
    /// </summary>
    public class SettingsModel
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("guid")]
        public Guid Guid { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("core")]
        public bool Core { get; set; }

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
        /// Name of an Umbraco property editor - full path is built later
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
        public string Tab { get; set; }

        /// <summary>
        /// The generated property alias
        /// </summary>
        [JsonProperty("alias")]
        public string Alias { get; set; }

        /// <summary>
        /// Prevalues for the setting
        /// </summary>
        [JsonProperty("prevalues")]
        public object Prevalues { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as SettingsModel;

            if (other == null) return false;
            if (other.Guid != Guid) return false;
            if (!other.Value.ToString().Equals(Value.ToString())) return false;

            return true;
        }

        /// <summary>
        /// Stops intellisense complaining
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => base.GetHashCode();        
    }


    /// <summary>
    /// Defines a generic Preflight setting
    /// </summary>
    public class GenericSettingModel : SettingsModel
    {
        public GenericSettingModel(string label, Guid guid)
        {
            Label = label;
            Alias = label.Camel();
            Guid = guid;
        }
    }

    /// <summary>
    /// Defines the Preflight setting for the disable/enable setting 
    /// </summary>
    internal class DisabledSettingModel : SettingsModel
    {
        public DisabledSettingModel(string tab, bool val, Guid guid)
        {
            Value = val ? KnownStrings.One : KnownStrings.Zero;
            Label = "Disabled";
            Alias = tab.DisabledAlias();
            Description = "Disable this plugin";
            View = SettingType.Boolean;
            Order = -10;
            Core = true;
            Tab = tab;
            Guid = guid;
        }
    }

    /// <summary>
    /// Defines the Preflight setting for the on-save-only setting 
    /// </summary>
    internal class OnSaveOnlySettingModel : SettingsModel
    {
        public OnSaveOnlySettingModel(string tab, bool val, Guid guid)
        {
            Value = val ? KnownStrings.One : KnownStrings.Zero;
            Label = "Run on save only";
            Alias = tab.OnSaveOnlyAlias();
            Description = "Restrict this plugin to run only in a save event";
            View = SettingType.Boolean;
            Order = -5;
            Core = true;
            Tab = tab;
            Guid = guid;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class PropertiesToTestSettingModel : SettingsModel
    {
        public PropertiesToTestSettingModel(string tab, string propsToTest, Guid guid)
        {
            Value = propsToTest;
            Label = "Properties to test";
            Alias = tab.PropertiesToTestAlias();
            Description = "Restrict this plugin to run against a subset of testable properties";
            View = SettingType.CheckboxList;
            Prevalues = KnownPropertyAlias.All.Select(x => new { value = x, key = x });
            Order = -15;
            Core = true;
            Tab = tab;
            Guid = guid;
        }
    }
}
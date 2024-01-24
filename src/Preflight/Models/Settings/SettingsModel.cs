using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Preflight.Models.Settings;

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
    /// A UI-friendly label for the setting.
    /// </summary>
    [JsonProperty("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Set the default value for the setting
    /// </summary>
    [JsonProperty("value")]
    public CaseInsensitiveValueDictionary? Value { get; set; }

    private object? _defaultValue;

    /// <summary>
    /// Applied to all existing languages as the default value
    /// </summary>
    [JsonIgnore]
    public object? DefaultValue
    {
        get => _defaultValue;
        set
        {
            SetValueDictionary(value);
            _defaultValue = value;
        }
    }

    /// <summary>
    /// Describe the setting
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Name of an Umbraco property editor - full path is built later
    /// </summary>
    [JsonProperty("view")]
    public string View { get; set; } = string.Empty;

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
    public string Tab { get; set; } = string.Empty;

    /// <summary>
    /// The generated property alias
    /// </summary>
    [JsonProperty("alias")]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Prevalues for the setting
    /// </summary>
    [JsonProperty("prevalues")]
    public object? Prevalues { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not SettingsModel other || other.Guid != Guid)
        {
            return false;
        }

        if (other?.Value?.Equals(Value) ?? false)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Stops intellisense complaining
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => base.GetHashCode();


    private void SetValueDictionary(object? val = null)
    {
        CaseInsensitiveValueDictionary dict = [];

        foreach (string lang in GetAllLanguages())
        {
            dict.Add(lang, val ?? DefaultValue);
        }

        Value = dict;
    }

    private IEnumerable<string> GetAllLanguages() =>
        StaticServiceProvider.Instance.GetRequiredService<ILocalizationService>().GetAllLanguages().Select(l => l.IsoCode);
}

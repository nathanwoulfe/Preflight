using Preflight.Models.Settings;

namespace Preflight.Plugins.Readability;

public interface IReadabilityService
{
    ReadabilityResponseModel Check(string text, string culture, List<SettingsModel> settings);
}

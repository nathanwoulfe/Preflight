using Preflight.Models;

namespace Preflight.Services;

public interface IReadabilityService
{
    ReadabilityResponseModel Check(string text, string culture, List<SettingsModel> settings);
}

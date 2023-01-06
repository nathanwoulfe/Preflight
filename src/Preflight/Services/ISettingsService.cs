using Preflight.Models.Settings;

namespace Preflight.Services;

public interface ISettingsService
{
    PreflightSettingsModel Get();

    bool Save(PreflightSettingsModel settings);
}

using Preflight.Models;

namespace Preflight.Services;

public interface ISettingsService
{
    PreflightSettings Get();

    bool Save(PreflightSettings settings);
}

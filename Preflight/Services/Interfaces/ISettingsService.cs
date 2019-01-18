using Preflight.Models;

namespace Preflight.Services.Interfaces
{
    public interface ISettingsService
    {
        PreflightSettings Get();

        bool Save(PreflightSettings settings);
    }
}

using System.Collections.Generic;
using Preflight.Models;

namespace Preflight.Services.Interfaces
{
    public interface ISettingsService
    {
        PreflightSettings Get();

        bool Save(List<SettingsModel> settings);
    }
}

using System.Collections.Generic;
using Preflight.Models;

namespace Preflight.Services.Interfaces
{
    public interface ISettingsService
    {
        List<SettingsModel> Get();

        bool Save(List<SettingsModel> settings);
    }
}

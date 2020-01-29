using Preflight.Models;
using System.Collections.Generic;

namespace Preflight.Services.Interfaces
{
    public interface IReadabilityService
    {
        ReadabilityResponseModel Check(string text, List<SettingsModel> settings);
    }
}

using Preflight.Models;

namespace Preflight.Services.Interfaces
{
    public interface IReadabilityService
    {
        ReadabilityResponseModel Check(string text);
    }
}

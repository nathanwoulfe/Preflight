using Preflight.Models;

namespace Preflight.Services;

public interface ISafeBrowsingService
{
    List<BrokenLinkModel> Check(string text, string apiKey);
}

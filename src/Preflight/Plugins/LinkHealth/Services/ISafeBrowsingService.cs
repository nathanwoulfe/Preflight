using Preflight.Plugins.LinkHealth.Models;

namespace Preflight.Plugins.LinkHealth.Services;

public interface ISafeBrowsingService
{
    List<BrokenLinkModel> Check(string text, string apiKey);
}

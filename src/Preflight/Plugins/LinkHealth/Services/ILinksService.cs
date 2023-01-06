using Preflight.Plugins.LinkHealth.Models;

namespace Preflight.Plugins.LinkHealth.Services;

public interface ILinksService
{
    List<BrokenLinkModel> Check(string text, List<BrokenLinkModel> safeBrowsingResult);
}

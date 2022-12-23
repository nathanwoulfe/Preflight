using Preflight.Models;

namespace Preflight.Services;

public interface ILinksService
{
    List<BrokenLinkModel> Check(string text, List<BrokenLinkModel> safeBrowsingResult);
}

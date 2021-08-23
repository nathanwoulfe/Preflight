using Preflight.Models;
using System.Collections.Generic;

namespace Preflight.Services
{
    public interface ISafeBrowsingService
    {
        List<BrokenLinkModel> Check(string text, string apiKey);
    }
}

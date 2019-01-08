using Preflight.Models;
using System.Collections.Generic;

namespace Preflight.Services.Interfaces
{
    public interface ISafeBrowsingService
    {
        List<BrokenLinkModel> Check(string text, string apiKey);
    }
}

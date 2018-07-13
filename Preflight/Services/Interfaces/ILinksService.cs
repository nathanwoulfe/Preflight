using System.Collections.Generic;
using Preflight.Models;

namespace Preflight.Services.Interfaces
{
    public interface ILinksService
    {
        List<BrokenLinkModel> Check(string text);
    }
}

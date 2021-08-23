using System.Threading.Tasks;
#if NET472
using Microsoft.AspNet.SignalR;
#else
using Microsoft.AspNetCore.SignalR;
#endif

namespace Preflight.Hubs
{
    public interface IPreflightHub
    {
        Task Refresh();
    }

    public class PreflightHub : Hub<IPreflightHub>
    {
    }
}
using Preflight.Models;
#if NETCOREAPP
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
#else
using Microsoft.AspNet.SignalR;
#endif

namespace Preflight.Hubs
{
    public interface IPreflightHub
    {

#if NETCOREAPP
        Task refresh();
        Task preflightTest(PreflightPropertyResponseModel model);
        Task preflightComplete();
#else
        void refresh();
        void preflightTest(PreflightPropertyResponseModel model);
        void preflightComplete();
#endif  
    }

    public class PreflightHub : Hub<IPreflightHub>
    {

    }
}
using System.Threading.Tasks;
using Preflight.Models;

#if NET472
using Microsoft.AspNet.SignalR;
#else
using Microsoft.AspNetCore.SignalR;
#endif

namespace Preflight.Hubs
{
    public interface IPreflightHub
    {

#if NET472
        void refresh();
        void preflightTest(PreflightPropertyResponseModel model);
        void preflightComplete();
#else
        Task refresh();
        Task preflightTest(PreflightPropertyResponseModel model);
        Task preflightComplete();
#endif  
    }

    public class PreflightHub : Hub<IPreflightHub>
    {

    }
}
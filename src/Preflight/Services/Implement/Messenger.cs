using Preflight.Hubs;
using Preflight.Models;
#if NETCOREAPP
using Microsoft.AspNetCore.SignalR;
#else
using Microsoft.AspNet.SignalR;
#endif

namespace Preflight.Services.Implement
{
    public class Messenger : IMessenger
    {
#if NETCOREAPP
        private readonly IHubContext<PreflightHub, IPreflightHub> _hubContext;

        public Messenger(IHubContext<PreflightHub, IPreflightHub> hubContext) => _hubContext = hubContext;        
#else
        private readonly IHubContext<IPreflightHub> _hubContext;

        public Messenger() => 
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<PreflightHub, IPreflightHub>();
#endif

        public void SendTestResult(PreflightPropertyResponseModel model) =>        
            _hubContext.Clients.All.preflightTest(model);        

        public void PreflightComplete() =>        
            _hubContext.Clients.All.preflightComplete();
        
    }
}

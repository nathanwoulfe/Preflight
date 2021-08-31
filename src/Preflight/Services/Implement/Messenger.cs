using Preflight.Hubs;
using Preflight.Models;
using System;
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
        private readonly Lazy<IHubContext<PreflightHub, IPreflightHub>> _hubContext;

        public Messenger(Lazy<IHubContext<PreflightHub, IPreflightHub>> hubContext) => _hubContext = hubContext;        
#else
        private readonly Lazy<IHubContext<IPreflightHub>> _hubContext;

        public Messenger(Lazy<IHubContext<IPreflightHub>> hubContext) => _hubContext = hubContext; 
#endif

        public void SendTestResult(PreflightPropertyResponseModel model) =>        
            _hubContext.Value.Clients.All.preflightTest(model);        

        public void PreflightComplete() =>        
            _hubContext.Value.Clients.All.preflightComplete();
        
    }
}

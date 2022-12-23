using Preflight.Hubs;
using Preflight.Models;
using Microsoft.AspNetCore.SignalR;

namespace Preflight.Services.Implement;

internal sealed class Messenger : IMessenger
{
    private readonly Lazy<IHubContext<PreflightHub, IPreflightHub>> _hubContext;

    public Messenger(Lazy<IHubContext<PreflightHub, IPreflightHub>> hubContext) => _hubContext = hubContext;

    public void SendTestResult(PreflightPropertyResponseModel model) =>
        _hubContext.Value.Clients.All.preflightTest(model);

    public void PreflightComplete() =>
        _hubContext.Value.Clients.All.preflightComplete();
}

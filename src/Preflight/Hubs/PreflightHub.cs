using Microsoft.AspNetCore.SignalR;
using Preflight.Models;

namespace Preflight.Hubs;

public interface IPreflightHub
{
    Task refresh();

    Task preflightTest(PreflightPropertyResponseModel model);

    Task preflightComplete(int nodeId);
}

internal sealed class PreflightHub : Hub<IPreflightHub>
{

}

using Microsoft.AspNetCore.SignalR;
using Preflight.Models;

namespace Preflight.Hubs;

public interface IPreflightHub
{
    Task refresh();

    Task preflightTest(PreflightPropertyResponseModel model);

    Task preflightComplete();
}

internal sealed class PreflightHub : Hub<IPreflightHub>
{

}

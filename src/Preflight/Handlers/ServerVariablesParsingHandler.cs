using Preflight.Executors;
using Preflight.Hubs;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Preflight.Handlers;

internal sealed class ServerVariablesParsingHandler : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly IServerVariablesParsingExecutor _executor;
    private readonly PreflightHubRoutes _preflightHubRoutes;

    public ServerVariablesParsingHandler(IServerVariablesParsingExecutor executor, PreflightHubRoutes preflightHubRoutes)
    {
        _executor = executor;
        _preflightHubRoutes = preflightHubRoutes;
    }

    public void Handle(ServerVariablesParsingNotification notification) =>
        _executor.Generate(notification.ServerVariables, new Dictionary<string, object>
        {
            { "signalRHub", _preflightHubRoutes.GetPreflightHubRoute() },
        });
}

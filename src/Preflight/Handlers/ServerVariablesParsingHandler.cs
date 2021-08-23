#if NET5_0
using Preflight.Executors;
using Preflight.Hubs;
using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Preflight.Handlers
{
    public class ServerVariablesParsingHandler : INotificationHandler<ServerVariablesParsingNotification>
    {
        private readonly IServerVariablesParsingExecutor _executor;
        private readonly PreflightHubRoutes _preflightHubRoutes;

        public ServerVariablesParsingHandler(IServerVariablesParsingExecutor executor, PreflightHubRoutes preflightHubRoutes)
        {
            _executor = executor ?? throw new System.ArgumentNullException(nameof(executor));
            _preflightHubRoutes = preflightHubRoutes ?? throw new System.ArgumentNullException(nameof(preflightHubRoutes));
        }

        public void Handle(ServerVariablesParsingNotification notification) =>
            _executor.Generate(notification.ServerVariables, new Dictionary<string, object> {
                { "signalRHub", _preflightHubRoutes.GetPreflightHubRoute() }
            });
    }
}
#endif
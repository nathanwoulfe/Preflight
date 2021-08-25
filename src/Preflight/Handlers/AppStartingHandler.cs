#if NET5_0
using Microsoft.Extensions.Logging;
using Preflight.Migrations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Preflight.Handlers
{
    public class AppStartingHandler : INotificationHandler<UmbracoApplicationStartingNotification>
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService;
        private readonly ILogger<Upgrader> _logger;

        public AppStartingHandler(
            IScopeProvider scopeProvider, 
            IKeyValueService keyValueService,
            IMigrationPlanExecutor migrationPlanExecutor, 
            ILogger<Upgrader> logger)
        {
            _scopeProvider = scopeProvider;
            _keyValueService = keyValueService;
            _migrationPlanExecutor = migrationPlanExecutor;
            _logger = logger;
        }

        public void Handle(UmbracoApplicationStartingNotification notification)
        {
            if (notification.RuntimeLevel < RuntimeLevel.Run)
            {
                _logger.LogInformation("Umbraco is not in run mode, Preflight will not run");
                return;
            }

            var upgrader = new Upgrader(new PreflightMigrationPlan());
            upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
        }
    }
}
#endif
#if NET472
using Preflight.Executors;
using Preflight.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Filters;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Editors;
using Umbraco.Web.JavaScript;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Preflight
{
    public class PreflightComponent : IComponent
    {
        private readonly IServerVariablesParsingExecutor _serverVariablesSendingExecutor;
        private readonly ISendingContentModelExecutor _sendingContentModelExecutor;
        private readonly IContentSavingExecutor _contentSavingExecutor;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly ILogger _logger;
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueService _keyValueService;

        public PreflightComponent(
            IServerVariablesParsingExecutor serverVariablesSendingExecutor,
            ISendingContentModelExecutor sendingContentModelExecutor,
            IContentSavingExecutor contentSavingExecutor,
            IMigrationBuilder migrationBuilder, 
            ILogger logger, 
            IScopeProvider scopeProvider, 
            IKeyValueService keyValueService)
        {
            _serverVariablesSendingExecutor = serverVariablesSendingExecutor ?? throw new ArgumentNullException(nameof(serverVariablesSendingExecutor));
            _sendingContentModelExecutor = sendingContentModelExecutor ?? throw new ArgumentNullException(nameof(sendingContentModelExecutor));
            _contentSavingExecutor = contentSavingExecutor ?? throw new ArgumentNullException(nameof(contentSavingExecutor));
            _migrationBuilder = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            _keyValueService = keyValueService ?? throw new ArgumentNullException(nameof(keyValueService));
        }

        public void Initialize()
        {
            //var upgrader = new Upgrader(new PreflightMigrationPlan());
            //upgrader.Execute(_scopeProvider, _migrationBuilder, _keyValueService, _logger);

            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
            ContentService.Saving += ContentService_Saving;
            EditorModelEventManager.SendingContentModel += EditorModelEventManager_SendingContentModel;
        }

        public void Terminate()
        {
            ServerVariablesParser.Parsing -= ServerVariablesParser_Parsing;
            ContentService.Saving -= ContentService_Saving;
            EditorModelEventManager.SendingContentModel -= EditorModelEventManager_SendingContentModel;
        }

        private void EditorModelEventManager_SendingContentModel(HttpActionExecutedContext sender, EditorModelEventArgs<ContentItemDisplay> e) =>
            _sendingContentModelExecutor.Execute(e.Model);
        

        /// <summary>
        /// Add preflight-specific values to the servervariables dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerVariablesParser_Parsing(object sender, IDictionary<string, object> e) =>
            _serverVariablesSendingExecutor.Generate(e);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentService_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            if (_contentSavingExecutor.SaveCancelledDueToFailedTests(e.SavedEntities.First(), out EventMessage message))
            {
                e.CancelOperation(message);
            }
        }
    }
}
#endif
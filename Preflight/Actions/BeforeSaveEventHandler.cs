using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClientDependency.Core;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using Umbraco.Core.Components;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Preflight.Actions
{
    public class BeforeSaveEventHandler : UmbracoComponentBase, IUmbracoUserComponent
    {
        protected void Initialize()
        {
            ContentService.Saving += Document_Saving;
        }

        /// <summary>
        /// Event Handler that gets hit before an item is Saved. 
        /// </summary>
        private static void Document_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            var settingsService = new SettingsService();

            List<SettingsModel> settings = settingsService.Get().Settings;
            int onSave = Convert.ToInt32(settings.First(s => s.Alias == KnownSettings.BindSaveHandler.Camel()).Value);
            int cancelOnFail = Convert.ToInt32(settings.First(s => s.Alias == KnownSettings.CancelSaveOnFail.Camel()).Value);

            if (onSave == 0) return;

            IContent content = e.SavedEntities.First();

            var checker = new ContentChecker();

            // perform autoreplace before readability check
            // only do this in save handler as there's no point in updating if it's not being saved (potentially)
            if (settings.Any(s => s.Alias == KnownSettings.RunAutoreplace.Camel() && s.Value.ToString() == "1"))
            {
                content = checker.Autoreplace(content);
            }

            PreflightResponseModel result = checker.Check(content);

            // at least one property on the current document fails the preflight check
            if (result.Failed == false) return;

            // these values are retrieved in the notifications handler, and passed down to the client
            HttpContext.Current.Items["PreflightResponse"] = result;
            HttpContext.Current.Items["PreflightNodeId"] = content.Id;

            if (cancelOnFail == 1)
            {
                e.Cancel = true;
            }
        }
    }
}

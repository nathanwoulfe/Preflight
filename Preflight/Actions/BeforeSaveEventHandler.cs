using System;
using System.Collections.Generic;
using System.Linq;
using Preflight.Models;
using Preflight.Services;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Preflight.Actions
{
    public class BeforeSaveEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Saving += Document_Saving;
        }

        /// <summary>
        /// Event Handler that gets hit before an item is Saved. 
        /// </summary>
        private static void Document_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            var settingsService = new SettingsService();

            List<SettingsModel> settings = settingsService.Get();
            int onSave = Convert.ToInt32(settings.First(s => s.Alias == "bindSaveHandler").Value);

            if (onSave == 0) return;

            IContent content = e.SavedEntities.First();

            var checker = new ContentChecker();
            PreflightResponseModel result = checker.Check(content);

            // at least one property on the current document fails the preflight check
            if (result.Failed == false) return;

            content.AdditionalData.Remove("SaveCancelled");
            content.AdditionalData.Remove("CancellationReason");
            content.AdditionalData.Remove("PreflightResponse");

            content.AdditionalData.Add("CancellationReason", Constants.ContentFailedChecks);
            content.AdditionalData.Add("PreflightResponse", result);
            content.AdditionalData.Add("SaveCancelled", DateTime.Now);

            e.Cancel = true;
        }
    }
}

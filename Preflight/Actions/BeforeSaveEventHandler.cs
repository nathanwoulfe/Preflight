using Preflight.Helpers;
using System;
using System.Linq;
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
        void Document_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            var settings = SettingsHelper.GetSettings();
            var onSave = Convert.ToInt32(settings.First(s => s.Alias == "bindSaveHandler").Value);

            if (onSave == 0) return;

            var content = e.SavedEntities.First();
            var checker = new ContentChecker();
            var result = checker.Check(content);

            // at least one property on the current document fails the preflight check
            if (!result.Failed) return;

            content.AdditionalData.Remove("SaveCancelled");
            content.AdditionalData.Remove("CancellationReason");
            content.AdditionalData.Remove("PreflightResponse");

            content.AdditionalData.Add("CancellationReason", "Content failed preflight checks");
            content.AdditionalData.Add("PreflightResponse", result);
            content.AdditionalData.Add("SaveCancelled", DateTime.Now);

            e.Cancel = true;
        }
    }
}

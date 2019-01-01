using System;
using System.Collections.Generic;
using System.Linq;
using Preflight.Constants;
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

            List<SettingsModel> settings = settingsService.Get();
            int onSave = Convert.ToInt32(settings.First(s => s.Alias == KnownSettingAlias.BindSaveHandler).Value);

            if (onSave == 0) return;

            IContent content = e.SavedEntities.First();

            var checker = new ContentChecker();

            // perform autoreplace before readability check
            // only do this in save handler as there's no point in updating if it's not being saved (potentially)
            if (settings.Any(s => s.Alias == KnownStrings.DoAutoreplace && s.Value.ToString() == "1"))
            {
                _ = checker.Autoreplace(content);
            }

            PreflightResponseModel result = checker.Check(content);

            // at least one property on the current document fails the preflight check
            if (result.Failed == false) return;

            // todo => v8 fix
            //e.AdditionalData.Remove("SaveCancelled");
            //e.AdditionalData.Remove("CancellationReason");
            //e.AdditionalData.Remove("PreflightResponse");

            //e.AdditionalData.Add("CancellationReason", KnownStrings.ContentFailedChecks);
            //e.AdditionalData.Add("PreflightResponse", result);
            //e.AdditionalData["SaveCancelled"] = DateTime.Now;

            e.Cancel = true;
        }
    }
}

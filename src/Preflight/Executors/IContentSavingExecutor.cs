using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
#if NET472
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Preflight.Security;
using CharArrays = Umbraco.Core.Constants.CharArrays;
#else
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;
#endif

namespace Preflight.Executors
{
    public interface IContentSavingExecutor
    {
        bool SaveCancelledDueToFailedTests(IContent content, out EventMessage message);
    }

    public class ContentSavingExecutor : IContentSavingExecutor
    {
        private readonly ISettingsService _settingsService;
        private readonly IContentChecker _contentChecker;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public ContentSavingExecutor(
            ISettingsService settingsService,
            IContentChecker contentChecker,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _contentChecker = contentChecker ?? throw new ArgumentNullException(nameof(contentChecker));
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
        }

        public bool SaveCancelledDueToFailedTests(IContent content, out EventMessage message)
        {
            message = null;

            List<SettingsModel> settings = _settingsService.Get().Settings;

            // only check if current user group is opted in to testing on save
            var groupSetting = settings.FirstOrDefault(x => string.Equals(x.Label, KnownSettings.UserGroupOptIn, StringComparison.InvariantCultureIgnoreCase));
            if (groupSetting != null && groupSetting.Value.HasValue())
            {
                var currentUserGroups = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser?.Groups?.Select(x => x.Name) ?? new List<string>();
                if (currentUserGroups.Any())
                {
                    bool testOnSave = groupSetting.Value.Split(CharArrays.Comma).Intersect(currentUserGroups).Any();

                    if (!testOnSave)
                        return false;
                }
            }

            if (!settings.GetValue<bool>(KnownSettings.BindSaveHandler)) return false;

            var cancelSaveOnFail = settings.GetValue<bool>(KnownSettings.CancelSaveOnFail);

            bool failed = _contentChecker.CheckContent(content, true);

            // at least one property on the current document fails the preflight check
            if (!failed) return false;

            // these values are retrieved in the notifications handler, and passed down to the client
            // TODO => make it work
            //HttpContext.Current.Items["PreflightFailed"] = true;
            //HttpContext.Current.Items["PreflightCancelSaveOnFail"] = cancelSaveOnFail;
            //HttpContext.Current.Items["PreflightNodeId"] = content.Id;

            //if (e.CanCancel && cancelSaveOnFail)
            //{
            //    e.CancelOperation(new EventMessage("PreflightFailed", content.Id.ToString()));
            //}

            message = new EventMessage("Save cancelled", content.Id.ToString());

            return true;
        }
    }
}

using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
#if NET472
using Preflight.Security;
using Umbraco.Web.Models.ContentEditing;
using CharArrays = Umbraco.Core.Constants.CharArrays;
#else
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Models.ContentEditing;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;
#endif

namespace Preflight.Executors
{
    public interface ISendingContentModelExecutor
    {
        void Execute(ContentItemDisplay contentItem);
    }

    public class SendingContentModelExecutor : ISendingContentModelExecutor
    {
        private readonly ISettingsService _settingsService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public SendingContentModelExecutor(ISettingsService settingsService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
        }

        public void Execute(ContentItemDisplay contentItem)
        {
            List<SettingsModel> settings = _settingsService.Get().Settings;

            var groupSetting = settings.FirstOrDefault(x => string.Equals(x.Label, KnownSettings.UserGroupOptIn, StringComparison.InvariantCultureIgnoreCase));
            var testablePropsSetting = settings.FirstOrDefault(x => string.Equals(x.Label, KnownSettings.PropertiesToTest, StringComparison.InvariantCultureIgnoreCase));

            if (groupSetting != null && groupSetting.Value.HasValue())
            {
                var currentUserGroups = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser?.Groups?.Select(x => x.Name) ?? new List<string>();
                if (currentUserGroups.Any())
                {
                    bool include = groupSetting.Value.Split(CharArrays.Comma).Intersect(currentUserGroups).Any();

                    if (!include)
                        contentItem.ContentApps = contentItem.ContentApps.Where(x => x.Name != KnownStrings.Name);
                }
            }

            // remove preflight app if content type doesn't include testable properties
            if (testablePropsSetting != null)
            {
                var defaultVariant = contentItem.Variants.FirstOrDefault();
                var properties = defaultVariant.Tabs.SelectMany(x => x.Properties.Select(y => y.Editor)).Distinct();

                var isTestable = properties.Intersect(testablePropsSetting.Value.Split(CharArrays.Comma)).Any();

                if (!isTestable)
                {
                    contentItem.ContentApps = contentItem.ContentApps.Where(x => x.Name != KnownStrings.Name);
                }
            }
        }
    }
}

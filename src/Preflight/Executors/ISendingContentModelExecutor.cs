using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.ContentEditing;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;
#else
using Preflight.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using CharArrays = Umbraco.Core.Constants.CharArrays;
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
        private readonly ILocalizationService _localizationService;

        public SendingContentModelExecutor(ISettingsService settingsService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, ILocalizationService localizationService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }

        public void Execute(ContentItemDisplay contentItem)
        {
            List<SettingsModel> settings = _settingsService.Get().Settings;

            // if node is invariant, use the default culture, else find the active language
            var culture = contentItem.Variants.Count() == 1 ? 
                _localizationService.GetDefaultLanguageIsoCode() : 
                contentItem.Variants.FirstOrDefault(x => x.Language.IsDefault).Language.IsoCode;

            var groupSetting = settings.GetValue<string>(KnownSettings.UserGroupOptIn, culture);
            var testablePropsSetting = settings.GetValue<string>(KnownSettings.PropertiesToTest, culture);

            if (groupSetting != null && groupSetting.HasValue())
            {
                var currentUserGroups = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser?.Groups?.Select(x => x.Name) ?? new List<string>();
                if (currentUserGroups.Any())
                {
                    bool include = groupSetting.Split(CharArrays.Comma).Intersect(currentUserGroups).Any();

                    if (!include)
                        contentItem.ContentApps = contentItem.ContentApps.Where(x => x.Name != KnownStrings.Name);
                }
            }

            // remove preflight app if content type doesn't include testable properties
            if (testablePropsSetting != null)
            {
                var defaultVariant = contentItem.Variants.FirstOrDefault();
                var properties = defaultVariant.Tabs.SelectMany(x => x.Properties.Select(y => y.Editor)).Distinct();

                var isTestable = properties.Intersect(testablePropsSetting.Split(CharArrays.Comma)).Any();

                if (!isTestable)
                {
                    contentItem.ContentApps = contentItem.ContentApps.Where(x => x.Name != KnownStrings.Name);
                }
            }
        }
    }
}

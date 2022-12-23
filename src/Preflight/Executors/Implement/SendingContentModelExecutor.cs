using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.ContentEditing;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;

namespace Preflight.Executors.Implement;

internal sealed class SendingContentModelExecutor : ISendingContentModelExecutor
{
    private readonly ISettingsService _settingsService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ILocalizationService _localizationService;

    public SendingContentModelExecutor(
        ISettingsService settingsService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILocalizationService localizationService)
    {
        _settingsService = settingsService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _localizationService = localizationService;
    }

    public void Execute(ContentItemDisplay contentItem)
    {
        List<SettingsModel> settings = _settingsService.Get().Settings;

        // if node is invariant, use the default culture, else find the active language
        string culture = contentItem.Variants.Count() == 1 ?
            _localizationService.GetDefaultLanguageIsoCode() :
            contentItem.Variants.First(x => x.Language!.IsDefault)!.Language!.IsoCode;

        string? groupSetting = settings.GetValue<string>(KnownSettings.UserGroupOptIn, culture);
        string? testablePropsSetting = settings.GetValue<string>(KnownSettings.PropertiesToTest, culture);

        if (groupSetting.HasValue())
        {
            IEnumerable<string?> currentUserGroups = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Groups?.Select(x => x.Name) ?? new List<string>();
            if (currentUserGroups.Any())
            {
                bool include = groupSetting.Split(CharArrays.Comma).Intersect(currentUserGroups).Any();

                if (!include)
                {
                    contentItem.ContentApps = contentItem.ContentApps
                        .Where(a => a.Alias is not null && !a.Alias.Equals(KnownStrings.Alias, StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        // remove preflight app if content type doesn't include testable properties
        if (testablePropsSetting is not null)
        {
            ContentVariantDisplay defaultVariant = contentItem.Variants.First();
            IEnumerable<string?> properties = defaultVariant.Tabs
                .SelectMany(x => x.Properties?.Select(y => y.Editor) ?? Enumerable.Empty<string>()).Distinct();

            bool isTestable = properties.Intersect(testablePropsSetting.Split(CharArrays.Comma)).Any();

            if (!isTestable)
            {
                contentItem.ContentApps = contentItem.ContentApps
                    .Where(a => a.Alias is not null && !a.Alias.Equals(KnownStrings.Alias, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}

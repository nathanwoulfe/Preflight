using Preflight.Extensions;
using Preflight.Models.Settings;
using Preflight.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;

namespace Preflight.Executors.Implement;

internal sealed class ContentSavingExecutor : IContentSavingExecutor
{
    private readonly ISettingsService _settingsService;
    private readonly IContentChecker _contentChecker;
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public ContentSavingExecutor(
        ISettingsService settingsService,
        IContentChecker contentChecker,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILocalizationService localizationService)
    {
        _settingsService = settingsService;
        _contentChecker = contentChecker;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _localizationService = localizationService;
    }

    public bool SaveCancelledDueToFailedTests(IContent content, out EventMessage? message)
    {
        message = null;

        List<SettingsModel> settings = _settingsService.Get().Settings;
        string culture = content.AvailableCultures.Any() ?
            content.AvailableCultures.First() :
            _localizationService.GetDefaultLanguageIsoCode();

        // only check if current user group is opted in to testing on save
        string? groupSetting = settings.GetValue<string>(KnownSettings.UserGroupOptIn, culture);
        if (groupSetting.HasValue())
        {
            IEnumerable<string?> currentUserGroups = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Groups?.Select(x => x.Name) ?? new List<string>();
            if (currentUserGroups.Any())
            {
                bool testOnSave = groupSetting.Split(CharArrays.Comma).Intersect(currentUserGroups).Any();

                if (!testOnSave)
                {
                    return false;
                }
            }
        }

        if (!settings.GetValue<bool>(KnownSettings.BindSaveHandler, culture))
        {
            return false;
        }

        bool cancelSaveOnFail = settings.GetValue<bool>(KnownSettings.CancelSaveOnFail, culture);

        bool failed = _contentChecker.CheckContent(content, culture, true);

        // at least one property on the current document fails the preflight check
        if (!failed)
        {
            return false;
        }

        message = new EventMessage(KnownStrings.ContentFailedChecks, $"PreflightCancelSaveOnFail_{cancelSaveOnFail}", EventMessageType.Error);

        return true;
    }
}

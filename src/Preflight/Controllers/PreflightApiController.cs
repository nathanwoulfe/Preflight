using Microsoft.AspNetCore.Mvc;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Models.Settings;
using Preflight.Services;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace Preflight.Controllers;

[PluginController(KnownStrings.Name)]
public class PreflightApiController : UmbracoAuthorizedApiController
{
    private readonly ISettingsService _settingsService;
    private readonly IContentChecker _contentChecker;
    private readonly ILocalizationService _localizationService;

    public PreflightApiController(
        ISettingsService settingsService,
        IContentChecker contentChecker,
        ILocalizationService localizationService)
    {
        _settingsService = settingsService;
        _contentChecker = contentChecker;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Get Preflight settings object
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetSettings()
    {
        try
        {
            return Ok(new
            {
                data = _settingsService.Get(),
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Save Preflight settings object
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public IActionResult SaveSettings(PreflightSettingsModel settings)
    {
        try
        {
            return Ok(new
            {
                data = _settingsService.Save(settings),
                notifications = ApiSuccessNotification("Settings updated"),
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Entry point for all content checking
    /// </summary>
    /// <param name="id">Node id</param>
    /// <param name="culture"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Check(int id, string culture = "")
    {
        try
        {
            return Ok(new
            {
                failed = _contentChecker.CheckContent(id, culture.HasValue() ? culture : _localizationService.GetDefaultLanguageIsoCode()),
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Entry point for checking sub-set of properties
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public IActionResult CheckDirty(DirtyProperties data)
    {
        try
        {
            if (data.Culture == string.Empty)
            {
                data.Culture = _localizationService.GetDefaultLanguageIsoCode();
            }

            return Ok(new
            {
                failed = _contentChecker.CheckDirty(data),
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private OkObjectResult Error(string message) =>
        Ok(new
        {
            notifications = ApiErrorNotification(message),
        });

    /// <summary>
    /// Gets an array of one BackofficeNotification
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private static BackOfficeNotification[] ApiSuccessNotification(string message) =>
        ApiNotification(message, "SUCCESS", NotificationStyle.Success);

    private static BackOfficeNotification[] ApiErrorNotification(string message) =>
        ApiNotification(message, "ERROR", NotificationStyle.Error);

    private static BackOfficeNotification[] ApiNotification(string message, string header, NotificationStyle style) =>
        [
            new()
            {
                NotificationType = style,
                Header = header,
                Message = message,
            },
        ];
}

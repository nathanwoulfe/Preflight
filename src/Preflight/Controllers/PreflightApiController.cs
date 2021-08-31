using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System;
using System.Net;
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using RoutePrefix = Microsoft.AspNetCore.Mvc.RouteAttribute;
#else
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using IActionResult = System.Web.Http.IHttpActionResult;
#endif

namespace Preflight.Controllers
{
    [RoutePrefix("umbraco/backoffice/preflight/settings")]
    [PluginController("Preflight")]
    public class ApiController : UmbracoAuthorizedApiController
    {
        private readonly ISettingsService _settingsService;
        private readonly IContentChecker _contentChecker;
        private readonly ILocalizationService _localizationService;

        public ApiController(ISettingsService settingsService, IContentChecker contentChecker, ILocalizationService localizationService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _contentChecker = contentChecker ?? throw new ArgumentNullException(nameof(contentChecker));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }

        /// <summary>
        /// Get Preflight settings object
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSettings")]
        public IActionResult GetSettings()
        {
            try
            {
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = _settingsService.Get()
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
        [Route("SaveSettings")]
        public IActionResult SaveSettings(PreflightSettings settings)
        {
            try
            {
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = _settingsService.Save(settings)
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
        /// <returns></returns>
        [HttpGet]
        [Route("Check/{id:int}/{culture?}")]
        public IActionResult Check(int id, string culture = "")
        {
            try
            {
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    failed = _contentChecker.CheckContent(id, culture.HasValue() ? culture : _localizationService.GetDefaultLanguageIsoCode())
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
        [Route("CheckDirty")]
        public IActionResult CheckDirty(DirtyProperties data)
        {
            try
            {
                if (data.Culture == string.Empty)
                    data.Culture = _localizationService.GetDefaultLanguageIsoCode();

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    failed = _contentChecker.CheckDirty(data)
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
        private IActionResult Error(string message)
        {
            return Ok(new
            {
                status = HttpStatusCode.InternalServerError,
                data = message
            });
        }
    }
}

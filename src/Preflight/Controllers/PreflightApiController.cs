using Preflight.Models;
using Preflight.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if NET472
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
#endif

namespace Preflight.Controllers
{
    [PluginController("Preflight")]
    public class ApiController : UmbracoAuthorizedApiController
    {
        private readonly ISettingsService _settingsService;
        private readonly IContentChecker _contentChecker;

        public ApiController(ISettingsService settingsService, IContentChecker contentChecker)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _contentChecker = contentChecker ?? throw new ArgumentNullException(nameof(contentChecker));
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
        /// Get Preflight settings object value
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSettingValue")]
        public IActionResult GetSettingValue(string id)
        {
            try
            {
                List<SettingsModel> settings = _settingsService.Get().Settings;
                SettingsModel model = settings.First(s => s.Alias == id);

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    value = model?.Value
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// Sabve Preflight settings object
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
        [Route("Check")]
        public IActionResult Check(int id)
        {
            try
            {
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    failed = _contentChecker.CheckContent(id)
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

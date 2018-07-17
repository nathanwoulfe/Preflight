using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Preflight.Services;
using Preflight.Services.Interfaces;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Preflight.Api
{
    [PluginController("preflight")]
    public class ApiController : UmbracoAuthorizedApiController 
    { 
        private readonly IContentService _contentService;
        private readonly ISettingsService _settingsService;
        private readonly ContentChecker _contentChecker;

        public ApiController() : this(new SettingsService(), ApplicationContext.Current.Services.ContentService, new ContentChecker())
        {
        }

        private ApiController(ISettingsService settingsService, IContentService contentService,
            ContentChecker contentChecker)
        {
            _settingsService = settingsService;
            _contentService = contentService;
            _contentChecker = contentChecker;
        }

        /// <summary>
        /// Get Preflight settings object
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getSettings")]
        public IHttpActionResult GetSettings()
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
                return Ok(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = ex.Message
                });
            }
        }

        /// <summary>
        /// Sabve Preflight settings object
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("saveSettings")]
        public IHttpActionResult SaveSettings(List<SettingsModel> settings)
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
                return Ok(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = ex.Message
                });
            }
        }

        /// <summary>
        /// Entry point for 
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("check/{id}")]
        public IHttpActionResult Check(int id)
        {            
            try
            {
                IContent content = _contentService.GetById(id);
                PreflightResponseModel response = _contentChecker.Check(content);

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = response
                });
            }
            catch(Exception ex)
            {
                return Ok(new
                {
                    status = HttpStatusCode.InternalServerError,
                    data = ex.Message
                });
            }
        }        
    }
}

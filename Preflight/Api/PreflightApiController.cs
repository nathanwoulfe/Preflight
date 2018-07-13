using Preflight.Helpers;
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

        public ApiController()
        {
            _settingsService = new SettingsService();
            _contentService = ApplicationContext.Current.Services.ContentService;
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
                var checker = new ContentChecker();

                IContent content = _contentService.GetById(id);
                PreflightResponseModel response = checker.Check(content);

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

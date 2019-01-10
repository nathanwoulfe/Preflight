using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Preflight.Services.Interfaces;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Preflight.Api
{
    [PluginController("preflight")]
    public class ApiController : UmbracoAuthorizedApiController 
    { 
        private readonly ISettingsService _settingsService;
        private readonly IContentChecker _contentChecker;

        public ApiController(ISettingsService settingsService, IContentChecker contentChecker)
        {
            _settingsService = settingsService;
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
        /// Entry point for all content checking
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("check/{id}")]
        public IHttpActionResult Check(int id)
        {            
            try
            {
                IContent content = Services.ContentService.GetById(id);
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

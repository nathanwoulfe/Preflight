using Preflight.Helpers;
using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Preflight.Api
{
    [PluginController("preflight")]
    public class ApiController : UmbracoAuthorizedApiController 
    { 
        private static readonly IContentService ContentService = ApplicationContext.Current.Services.ContentService;

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
                    data = SettingsHelper.GetSettings()
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
                    data = SettingsHelper.SaveSettings(settings)
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
                var content = ContentService.GetById(id);
                var checker = new ContentChecker();
                var response = checker.Check(content);

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

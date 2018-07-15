using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.UI;
using Umbraco.Web.Editors;
using System.Web.Mvc;
using Umbraco.Web;

namespace Preflight.Actions
{
    public class CustomNotificationsHandler : IApplicationEventHandler
    {
        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            GlobalConfiguration.Configuration.MessageHandlers.Add(new WebApiHandler());
        }
    }

    /// <summary>
    /// If Preflight has cancelled the save, remove the default Umbraco cancellation notification and replace it with a new one, to intercept on the client side
    /// </summary>
    public class WebApiHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var urlHelper =
            //    new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            //string url = urlHelper.GetUmbracoApiServiceBaseUrl<ContentController>(controller => controller.PostSave(null));

            if (request.RequestUri.AbsolutePath.ToLower() == "/umbraco/backoffice/umbracoapi/content/postsave")
            {
                return base.SendAsync(request, cancellationToken)
                    .ContinueWith(task =>
                    {
                        HttpResponseMessage response = task.Result;
                        try
                        {
                            HttpContent data = response.Content;
                            ContentItemDisplay content = ((ObjectContent)data).Value as ContentItemDisplay;

                            if (content != null && content.AdditionalData.ContainsKey("SaveCancelled") && content.AdditionalData.ContainsKey("PreflightResponse"))
                            {
                                // if preflight response exists, ditch all other notifications
                                content.Notifications.Clear();
                                content.Notifications.Add(new Notification
                                {
                                    Header = Constants.ContentFailedChecks,
                                    Message = JsonConvert.SerializeObject(content.AdditionalData["PreflightResponse"]),
                                    NotificationType = SpeechBubbleIcon.Error
                                });
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error<WebApiHandler>("Error changing custom publishing cancelled message.", ex);
                        }
                        return response;

                    }, cancellationToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

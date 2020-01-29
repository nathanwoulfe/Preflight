using Preflight.Constants;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Web.Models.ContentEditing;

namespace Preflight
{
    /// <summary>
    /// If Preflight has cancelled the save, remove the default Umbraco cancellation notification and replace it with a new one, to intercept on the client side
    /// </summary>
    public class NotificationsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsolutePath.ToLower() == "/umbraco/backoffice/umbracoapi/content/postsave")
            {
                return base.SendAsync(request, cancellationToken)
                    .ContinueWith(task =>
                    {
                        HttpResponseMessage response = task.Result;
                        try
                        {
                            request.Properties.TryGetValue("MS_HttpContext", out object contextObject);
                            var context = contextObject as HttpContextWrapper;

                            if (context == null || !context.Items.Contains("PreflightFailed"))
                                return response;

                            HttpContent data = response.Content;
                            var content = ((ObjectContent)data).Value as ContentItemDisplay;

                            if (content != null && context.Items["PreflightNodeId"] as int? == content.Id)
                            {
                                // if preflight response exists, ditch all other notifications
                                // this exists as a hook for interception on the client
                                content.Notifications.Clear();
                                content.Notifications.Add(new Notification
                                {
                                    Header = KnownStrings.ContentFailedChecks,
                                    Message = $"PreflightCancelSaveOnFail_{context.Items["PreflightCancelSaveOnFail"]}",
                                    NotificationType = NotificationStyle.Error
                                });

                            }
                        }
                        catch (Exception ex)
                        {
                            //todo => v8 logging
                            //LogHelper.Error<WebApiHandler>("Error changing custom publishing cancelled message.", ex);
                        }
                        return response;

                    }, cancellationToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

#if NETCOREAPP
using Umbraco.Cms.Core.Models.ContentEditing;
#else
using Umbraco.Web.Models.ContentEditing;
using BackOfficeNotification = Umbraco.Web.Models.ContentEditing.Notification;
#endif

namespace Preflight.Models
{
    /// <summary>
    /// This is a temporary workaround to deal with a naming mismatch in the Umbraco notifications
    /// </summary>
    public class WrappedNotification : BackOfficeNotification
    {
        public NotificationStyle Type { get; set; }

        public WrappedNotification(BackOfficeNotification notification) : base()
        {
            Header = notification.Header;
            Message = notification.Message;
            NotificationType = notification.NotificationType;
            Type = notification.NotificationType;
        }
    }
}

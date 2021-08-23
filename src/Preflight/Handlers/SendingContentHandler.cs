#if NET5_0
using Preflight.Executors;
using System;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Preflight.Handlers
{
    public class SendingContentHandler : INotificationHandler<SendingContentNotification>
    {
        private readonly ISendingContentModelExecutor _executor;

        public SendingContentHandler(ISendingContentModelExecutor executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public void Handle(SendingContentNotification notification) =>
            _executor.Execute(notification.Content);
    }
}
#endif
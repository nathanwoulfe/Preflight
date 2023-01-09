using Preflight.Executors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Preflight.Handlers;

internal sealed class SendingContentHandler : INotificationHandler<SendingContentNotification>
{
    private readonly ISendingContentModelExecutor _executor;

    public SendingContentHandler(ISendingContentModelExecutor executor)
    {
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    public void Handle(SendingContentNotification notification)
    {
        if (notification.Content.Id == 0)
        {
            return;
        }

        _executor.Execute(notification.Content);
    }
}

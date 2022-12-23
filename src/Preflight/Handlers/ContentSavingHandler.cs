using Preflight.Executors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Preflight.Handlers;

internal sealed class ContentSavingHandler : INotificationHandler<ContentSavingNotification>
{
    private readonly IContentSavingExecutor _executor;

    public ContentSavingHandler(IContentSavingExecutor executor)
    {
        _executor = executor ?? throw new System.ArgumentNullException(nameof(executor));
    }

    public void Handle(ContentSavingNotification notification)
    {
        if (_executor.SaveCancelledDueToFailedTests(notification.SavedEntities.First(), out EventMessage? message))
        {
            notification.CancelOperation(message!);
        }
    }
}

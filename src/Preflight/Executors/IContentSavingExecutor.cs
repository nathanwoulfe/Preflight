using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Preflight.Executors;

public interface IContentSavingExecutor
{
    bool SaveCancelledDueToFailedTests(IContent content, out EventMessage? message);
}

using Umbraco.Cms.Core.Models.ContentEditing;

namespace Preflight.Executors;
public interface ISendingContentModelExecutor
{
    void Execute(ContentItemDisplay contentItem);
}

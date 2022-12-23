using Preflight.Models;


namespace Preflight.Executors;

public interface IPluginExecutor
{
    PreflightPropertyResponseModel Execute(ContentParserParams parserParams, string parentAlias = "");
}

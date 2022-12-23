namespace Preflight.Executors;

public interface IServerVariablesParsingExecutor
{
    void Generate(IDictionary<string, object> dictionary, IDictionary<string, object>? additionalData);
}

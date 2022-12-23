namespace Preflight.Services;

public interface ICacheManager
{
    void Set(string key, object thing);

    bool TryGet<T>(string key, out T thing);
}

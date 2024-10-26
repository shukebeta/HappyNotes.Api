namespace HappyNotes.Services.interfaces;

public interface IGeneralMemoryCacheService
{
    T? Get<T>(string cacheKey);
    void Set<T>(string cacheKey, T cacheValue, TimeSpan? expiration = null);
    void ClearCache(string cacheKey);
}

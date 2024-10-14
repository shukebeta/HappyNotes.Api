using Api.Framework;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HappyNotes.Services;

public class MastodonUserAccountCacheService(
    IMemoryCache cache,
    IRepositoryBase<MastodonUserAccount> mastodonUserAccountsRepository)
    : IMastodonUserAccountCacheService
{
    private static string CacheKey(long userId) => $"MUA_{userId}";

    // Set cache options
    private static readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(1440)); // Set expiration time

    public async Task<IList<MastodonUserAccount>> GetAsync(long userId)
    {
        if (cache.TryGetValue(CacheKey(userId), out List<MastodonUserAccount>? config))
        {
            return config!;
        }

        // If not in cache, load from the database
        var settings = await mastodonUserAccountsRepository.GetListAsync(
            s => s.UserId == userId && s.Status == MastodonUserAccountStatus.Created);

        Set(userId, settings);
        return settings;
    }

    public void Set(long userId, IList<MastodonUserAccount> settings)
    {
        cache.Set(CacheKey(userId), settings, CacheEntryOptions);
    }


    public void ClearCache(long userId)
    {
        cache.Remove(CacheKey(userId));
    }
}

using Api.Framework;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HappyNotes.Services;

public class TelegramSettingsCacheService(
    IMemoryCache cache,
    IRepositoryBase<TelegramSettings> telegramSettingsRepository)
    : ITelegramSettingsCacheService
{
    private static string CacheKey(long userId) => $"VTS_{userId}";

    // Set cache options
    private static readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(10)); // Set expiration time

    public async Task<IList<TelegramSettings>> GetAsync(long userId)
    {
        if (cache.TryGetValue(CacheKey(userId), out List<TelegramSettings>? config))
        {
            return config!;
        }

        // If not in cache, load from the database
        var settings = await telegramSettingsRepository.GetListAsync(
            s => s.UserId == userId && s.Status == TelegramSettingStatus.Normal);

        Set(userId, settings);
        return settings;
    }

    public void Set(long userId, IList<TelegramSettings> settings)
    {
        cache.Set(CacheKey(userId), settings, CacheEntryOptions);
    }


    public void ClearCache(long userId)
    {
        cache.Remove(CacheKey(userId));
    }
}

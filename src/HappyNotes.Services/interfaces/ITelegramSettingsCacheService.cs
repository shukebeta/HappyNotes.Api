using HappyNotes.Entities;

namespace HappyNotes.Services.interfaces;

public interface ITelegramSettingsCacheService
{
    Task<IList<TelegramSettings>> GetAsync(long userId);
    void Set(long userId, IList<TelegramSettings> settings);
    void ClearCache(long userId);
}

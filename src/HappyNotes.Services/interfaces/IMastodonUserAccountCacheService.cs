using HappyNotes.Entities;

namespace HappyNotes.Services.interfaces;

public interface IMastodonUserAccountCacheService
{
    Task<IList<MastodonUserAccount>> GetAsync(long userId);
    void Set(long userId, IList<MastodonUserAccount> account);
    void ClearCache(long userId);
}

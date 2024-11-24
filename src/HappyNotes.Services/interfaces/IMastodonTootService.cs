
using Mastonet.Entities;

namespace HappyNotes.Services.interfaces;

public interface IMastodonTootService
{
    Task<Status> SendTootAsync(string instanceUrl, string accessToken, string text, bool isPrivate, bool isMarkdown=false);
    Task<Status> EditTootAsync(string instanceUrl, string accessToken, string tootId, string newText, bool isPrivate, bool isMarkdown=false);
    Task<Status> SendLongTootAsPhotoAsync(string instanceUrl, string accessToken, string longText, bool isMarkdown, bool isPrivate);
    Task DeleteTootAsync(string instanceUrl, string accessToken, string tootId);
}

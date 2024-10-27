using HappyNotes.Services.interfaces;
using Mastonet;
using Mastonet.Entities;

namespace HappyNotes.Services;

public class MastodonTootService : IMastodonTootService
{
    public async Task<Status> SendTootAsync(string instanceUrl, string accessToken, string text, bool isPrivate)
    {
        var client = new MastodonClient(instanceUrl, accessToken);

        return await client.PublishStatus(text, isPrivate ? Visibility.Private: Visibility.Public);
    }

    public async Task<Status> EditTootAsync(string instanceUrl, string accessToken, string tootId, string newText, bool isPrivate)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        var visibility = isPrivate ? Visibility.Private : Visibility.Public;
        var toot = await client.GetStatus(tootId);
        if (toot.Visibility != visibility)
        {
            await client.DeleteStatus(tootId);
            return await client.PublishStatus(newText, visibility);
        }

        return await client.EditStatus(tootId, newText);
    }

    // public async Task<Status> SendLongTootAsPhotoAsync(string instanceUrl, string accessToken, string longText, bool isPrivate)
    // {
    //     var client = new MastodonClient(instanceUrl, accessToken);
    //     var filePath = Path.GetTempFileName();
    //     await File.WriteAllTextAsync(filePath, longText);
    //
    //     using var stream = File.OpenRead(filePath);
    //     return await client.PostMedia(stream, "text/plain", "long_toot.txt");
    // }
    //
    public async Task DeleteTootAsync(string instanceUrl, string accessToken, string tootId)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        await client.DeleteStatus(tootId);
    }
}

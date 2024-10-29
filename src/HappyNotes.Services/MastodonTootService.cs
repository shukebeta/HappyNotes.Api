using System.Drawing.Imaging;
using CoreHtmlToImage;
using HappyNotes.Common;
using HappyNotes.Services.interfaces;
using Markdig;
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

    public async Task<Status> SendLongTootAsPhotoAsync(string instanceUrl, string accessToken, string longText, bool isPrivate)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        var filePath = Path.GetTempFileName();

        try
        {
            var converter = new HtmlConverter();
            string htmlContent = Markdown.ToHtml(longText);
            htmlContent = $"<html lang=\"en\" dir=\"ltr\">\n<head>\n<meta charset=\"UTF-8\">\n<link rel=\"stylesheet\" href=\"https://files.shukebeta.com/markdown.css\" />\n</head>\n<body>\n{htmlContent}</body></html>";
            var bytes = converter.FromHtmlString(htmlContent, width: 600);
            var memoryStream = new MemoryStream(bytes);
            var media = await client.UploadMedia(memoryStream, "long_text.jpg");

            return await client.PublishStatus(
                "Tap the image for full content. 点击图片查看全文。",
                isPrivate ? Visibility.Private : Visibility.Public,
                mediaIds: [media.Id,]
            );
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    public async Task DeleteTootAsync(string instanceUrl, string accessToken, string tootId)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        await client.DeleteStatus(tootId);
    }
}

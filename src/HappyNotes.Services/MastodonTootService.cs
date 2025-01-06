using System.Text.RegularExpressions;
using CoreHtmlToImage;
using HappyNotes.Common;
using HappyNotes.Services.interfaces;
using Markdig;
using Mastonet;
using Mastonet.Entities;

namespace HappyNotes.Services;

public class MastodonTootService : IMastodonTootService
{
    private static readonly Regex ImagePattern = new(@"!\[(.*?)\]\((.*?)\)", RegexOptions.Compiled);
    private const int MaxImages = 4;

    public async Task<Status> SendTootAsync(string instanceUrl, string accessToken, string text, bool isPrivate,
        bool isMarkdown = false)
    {
        var client = new MastodonClient(instanceUrl, accessToken);

        if (!isMarkdown) return await client.PublishStatus(text, isPrivate ? Visibility.Private : Visibility.Public);
        var (processedText, mediaIds) = await ProcessMarkdownImagesAsync(client, text);

        return await client.PublishStatus(
            processedText,
            visibility: isPrivate ? Visibility.Private : Visibility.Public,
            mediaIds: mediaIds
        );
    }

    public async Task<Status> EditTootAsync(
        string instanceUrl,
        string accessToken,
        string tootId,
        string newText,
        bool isPrivate,
        bool isMarkdown = false)
    {
        if (newText.Length <= Constants.MastodonTootLength)
            return await _EditShortToot(instanceUrl, accessToken, tootId, newText, isPrivate, isMarkdown);
        return await _EditLongTootAsPhotoAsync(instanceUrl, accessToken, tootId, newText, isPrivate, isMarkdown);
    }

    private async Task<Status> _EditShortToot(string instanceUrl, string accessToken, string tootId, string newText,
        bool isPrivate,
        bool isMarkdown)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        var visibility = isPrivate ? Visibility.Private : Visibility.Public;

        // Get original toot
        var toot = await client.GetStatus(tootId);

        // Process markdown if enabled
        var processedText = newText;
        IEnumerable<string> mediaIds = Array.Empty<string>();

        if (isMarkdown)
        {
            (processedText, mediaIds) = await ProcessMarkdownImagesAsync(client, newText);
        }

        // If visibility changed, delete and recreate
        if (toot.Visibility != visibility)
        {
            await client.DeleteStatus(tootId);
            return await client.PublishStatus(
                processedText,
                visibility: visibility,
                mediaIds: mediaIds
            );
        }

        // Otherwise, update existing toot
        return await client.EditStatus(
            tootId,
            processedText,
            mediaIds: isMarkdown ? mediaIds : null // Only include mediaIds if markdown was processed
        );
    }

    public async Task<Status> SendLongTootAsPhotoAsync(string instanceUrl, string accessToken, string longText,
        bool isMarkdown, bool isPrivate)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        var filePath = Path.GetTempFileName();

        try
        {
            var media = await _UploadLongTextAsMedia(client, longText, isMarkdown);

            return await client.PublishStatus(
                string.Empty,
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

    private static async Task<Attachment> _UploadLongTextAsMedia(MastodonClient client, string longText, bool isMarkdown)
    {
        var htmlContent = isMarkdown ? Markdown.ToHtml(longText) : $"<p>{longText}</p>";
        htmlContent =
            $"<html lang=\"en\">\n<head>\n<meta charset=\"UTF-8\">\n<link rel=\"stylesheet\" href=\"https://files.shukebeta.com/markdown.css\" />\n</head>\n<body>\n{htmlContent}</body></html>";
        var converter = new HtmlConverter();
        var bytes = converter.FromHtmlString(htmlContent, width: 600);
        var memoryStream = new MemoryStream(bytes);
        var media = await client.UploadMedia(memoryStream, "long_text.jpg");
        return media;
    }

    public async Task DeleteTootAsync(string instanceUrl, string accessToken, string tootId)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        await client.DeleteStatus(tootId);
    }

    private async Task<Status> _EditLongTootAsPhotoAsync(
        string instanceUrl,
        string accessToken,
        string tootId,
        string longText,
        bool isPrivate,
        bool isMarkdown)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        var toot = await client.GetStatus(tootId);
        var visibility = isPrivate ? Visibility.Private : Visibility.Public;

        try
        {

            var media = await _UploadLongTextAsMedia(client, longText, isMarkdown);

            // If visibility changed, we need to delete and recreate
            if (toot.Visibility != visibility)
            {
                await client.DeleteStatus(tootId);
                return await client.PublishStatus(
                    string.Empty,
                    visibility: visibility,
                    mediaIds: [media.Id]
                );
            }

            // Otherwise, edit the existing toot
            return await client.EditStatus(
                tootId,
                string.Empty,
                mediaIds: [media.Id]
            );
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to edit long toot as photo: {ex.Message}", ex);
        }
    }

    private async Task<(string text, IEnumerable<string> mediaIds)> ProcessMarkdownImagesAsync(
        MastodonClient client,
        string markdownText)
    {
        var allMatches = ImagePattern.Matches(markdownText);
        var picCount = allMatches.Count;
        if (picCount == 0)
        {
            return (markdownText, Array.Empty<string>());
        }

        var matches = allMatches
            .Take(MaxImages)
            .ToList();

        var uploadTasks = new List<Task<(int index, Attachment attachment)>>();

        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var altText = match.Groups[1].Value.Trim();
            var imageUrl = match.Groups[2].Value;
            var index = i;
            uploadTasks.Add(UploadImageAsync(client, imageUrl, altText, index));
        }

        var results = await Task.WhenAll(uploadTasks);

        // Sort by original index to maintain order
        var mediaIds = results.OrderBy(r => r.index)
            .Select(r => r.attachment.Id)
            .ToList();

        // Replace markdown image syntax with reference text including alt text
        var processedText = markdownText;
        for (var i = 0; i < matches.Count; i++)
        {
            var altText = matches[i].Groups[1].Value.Trim();
            altText = altText.Equals("image") ? string.Empty : altText;
            var imageText = !string.IsNullOrWhiteSpace(altText)
                ? $"image {i + 1}: {altText}"
                : $"image {i + 1}";
            if (picCount == 1)
            {
                imageText = !string.IsNullOrWhiteSpace(altText)
                    ? $"image: {altText}"
                    : string.Empty;
            }

            processedText = processedText.Replace(
                matches[i].Value,
                imageText
            );
            if (i == 0 && processedText.Trim().Equals(imageText))
            {
                return (string.Empty, mediaIds);
            }
        }

        return (processedText, mediaIds);
    }

    private async Task<(int index, Attachment attachment)> UploadImageAsync(
        MastodonClient client,
        string imageUrl,
        string altText,
        int index)
    {
        try
        {
            using var httpClient = new HttpClient();
            await using var imageStream = await httpClient.GetStreamAsync(imageUrl);

            // Extract filename from URL or use default
            var filename = string.Join("_",
                imageUrl.Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Last()
                    .Split('?')[0]
                    .Split('#')[0]
                    .Take(20)
            );

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = $"image_{index}.jpg";
            }

            // Upload with alt text if provided
            var attachment = !string.IsNullOrWhiteSpace(altText)
                ? await client.UploadMedia(imageStream, filename, altText)
                : await client.UploadMedia(imageStream, filename);

            return (index, attachment);
        }
        catch (Exception ex)
        {
            // Log error here if needed
            throw new Exception($"Failed to upload image {imageUrl}: {ex.Message}", ex);
        }
    }
}

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
    private static readonly Regex ImagesSuffixPattern = new(@"((?:\s*image\s*\d\s*)+)", RegexOptions.Compiled);
    private const int MaxImages = 4;
    private static string _GetStringTags(string longText) => string.Join(' ', longText.GetTags().Where(t => !t.StartsWith("@")).Select(t => $"#{t}"));

    public async Task<Status> SendTootAsync(string instanceUrl, string accessToken, string text, bool isPrivate,
        bool isMarkdown = false)
    {
        var fullText = _GetFullText(text, isMarkdown);
        if (fullText.Length > Constants.MastodonTootLength)
        {
            return await _SendLongTootAsPhotoAsync(instanceUrl, accessToken, fullText, isPrivate, isMarkdown);
        }

        var client = new MastodonClient(instanceUrl, accessToken);
        if (!isMarkdown) return await client.PublishStatus(fullText, isPrivate ? Visibility.Private : Visibility.Public);
        var (processedText, mediaIds) = await ProcessMarkdownImagesAsync(client, fullText);

        return await client.PublishStatus(
            processedText,
            visibility: isPrivate ? Visibility.Private : Visibility.Public,
            mediaIds: mediaIds
        );
    }

    private static async Task<Status> _SendLongTootAsPhotoAsync(string instanceUrl, string accessToken, string longText,
        bool isPrivate, bool isMarkdown)
    {
        var client = new MastodonClient(instanceUrl, accessToken);
        var filePath = Path.GetTempFileName();

        try
        {
            var media = await _UploadLongTextAsMedia(client, longText, isMarkdown);

            return await client.PublishStatus(
                _GetStringTags(longText),
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

    public async Task<Status> EditTootAsync(
        string instanceUrl,
        string accessToken,
        string tootId,
        string newText,
        bool isPrivate,
        bool isMarkdown = false)
    {
        var fullText = _GetFullText(newText, isMarkdown);
        if (fullText.Length > Constants.MastodonTootLength)
        {
            return await _EditLongTootAsPhotoAsync(instanceUrl, accessToken, tootId, fullText, isPrivate, isMarkdown);
        }

        var client = new MastodonClient(instanceUrl, accessToken);
        var visibility = isPrivate ? Visibility.Private : Visibility.Public;

        // Get original toot
        var toot = await client.GetStatus(tootId);

        // Process markdown if enabled
        var processedText = fullText;
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

    private static string _GetFullText(string text, bool isMarkdown)
    {
        if (isMarkdown && text.Length > Constants.MastodonTootLength)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()  // This includes tables
                .Build();
            return Markdown.ToHtml(text, pipeline);
        }
        return text;
    }

    private static async Task<Attachment> _UploadLongTextAsMedia(MastodonClient client, string longText, bool isMarkdown)
    {
        var htmlContent = isMarkdown ? longText : longText.Replace("\n", "<br>");
        htmlContent =
            $"<html lang=\"en\">\n<head>\n<meta charset=\"UTF-8\">\n<link rel=\"stylesheet\" href=\"https://files.shukebeta.com/markdown.css?v2\" />\n</head>\n<body>\n{htmlContent}</body></html>";
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

    private static async Task<Status> _EditLongTootAsPhotoAsync(
        string instanceUrl,
        string accessToken,
        string tootId,
        string longText,
        bool isPrivate,
        bool isMarkdown)
    {
        try
        {
            var client = new MastodonClient(instanceUrl, accessToken);
            var toot = await client.GetStatus(tootId);
            var visibility = isPrivate ? Visibility.Private : Visibility.Public;
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
                _GetStringTags(longText),
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
        var imagesTextMatch = ImagesSuffixPattern.Match(processedText);
        if (imagesTextMatch.Success)
        {
            processedText = processedText.Replace(imagesTextMatch.Value, " ").Trim();
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

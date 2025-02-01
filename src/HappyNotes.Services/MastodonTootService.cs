using CoreHtmlToImage;
using HappyNotes.Common;
using HappyNotes.Services.interfaces;
using Markdig;
using Mastonet;
using Mastonet.Entities;
using Microsoft.Extensions.Logging;

namespace HappyNotes.Services;

public class MastodonTootService(ILogger<MastodonTootService> logger
, IHttpClientFactory httpClientFactory) : IMastodonTootService
{
    private const int MaxImages = 4;

    private static string _GetStringTags(string longText) =>
        string.Join(' ', longText.GetTags().Where(t => !t.StartsWith("@")).Select(t => $"#{t}"));

    public async Task<Status> SendTootAsync(string instanceUrl, string accessToken, string text, bool isPrivate,
        bool isMarkdown = false)
    {
        var fullText = _GetFullText(text, isMarkdown);
        if (fullText.Length > Constants.MastodonTootLength)
        {
            return await _SendLongTootAsPhotoAsync(instanceUrl, accessToken, fullText, isPrivate, isMarkdown);
        }

        var client = new MastodonClient(instanceUrl, accessToken);
        if (!isMarkdown)
            return await client.PublishStatus(fullText, isPrivate ? Visibility.Private : Visibility.Public);
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
                .UseAdvancedExtensions() // This includes tables
                .Build();
            return Markdown.ToHtml(text, pipeline);
        }

        return text;
    }

    private static async Task<Attachment> _UploadLongTextAsMedia(MastodonClient client, string longText,
        bool isMarkdown)
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
        var imgInfos = markdownText.GetImgInfos();
        if (imgInfos.Count == 0)
        {
            return (markdownText, Array.Empty<string>());
        }

        var matches = imgInfos
            .Take(MaxImages)
            .ToList();

        var uploadTasks = new List<Task<(int index, Attachment attachment)>>();

        for (var index = 0; index < matches.Count; index++)
        {
            uploadTasks.Add(UploadImageAsync(client, matches[index].ImgUrl, matches[index].Alt, index));
        }

        var successfulUploads = new List<(int index, Attachment attachment)>();

        try
        {
            var results = await Task.WhenAll(uploadTasks);
            // if all good, add to successfulUploads
            successfulUploads.AddRange(results);
        }
        catch
        {
            // collect successful results only from all tasks
            for(var i = 0; i < uploadTasks.Count; i++)
            {
                var task = uploadTasks[i];
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    successfulUploads.Add(await task);
                }
                else if (task.Status == TaskStatus.Faulted)
                {
                    logger.LogError(task.Exception, $"Failed to upload image: {matches[i].ImgUrl}");
                }
            }
        }

        // Sort by original index to maintain order, but only keep the successful uploads
        var mediaIds = successfulUploads
            .OrderBy(r => r.index)
            .Select(r => r.attachment.Id)
            .ToList();

        if (successfulUploads.Count < matches.Count)
        {
            // todo: collect all failure images and send a message to user that some images failed to upload
        }

        // Replace markdown image syntax with reference text including alt text
        foreach (var task in successfulUploads)
        {
            var altText = matches[task.index].Alt;
            altText = altText.Equals("image") ? string.Empty : altText;

            var imageText = !string.IsNullOrWhiteSpace(altText)
                ? $"image {task.index + 1}: {altText}"
                : $"image {task.index + 1}";
            // if there is only one picture
            if (successfulUploads.Count == 1)
            {
                imageText = !string.IsNullOrWhiteSpace(altText)
                    ? $"image: {altText}"
                    : string.Empty;
                if (markdownText.Trim().Equals(imageText))
                {
                    return (string.Empty, mediaIds);
                }
            }

            markdownText = markdownText.Replace(
                matches[task.index].Match.Value,
                imageText
            );
        }

        return (markdownText.RemoveImageReference(), mediaIds);
    }

    private async Task<(int index, Attachment attachment)> UploadImageAsync(
        MastodonClient client,
        string imageUrl,
        string altText,
        int index)
    {
        const int maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(1);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var httpClient = httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

                // Validate response
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch image. Status code: {response.StatusCode}");
                }

                // Verify content type
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("image/"))
                {
                    throw new Exception($"Invalid content type: {contentType}. Expected image/*");
                }

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

                // Read the content as a stream
                await using var imageStream = await response.Content.ReadAsStreamAsync();

                // Upload with alt text if provided
                var attachment = !string.IsNullOrWhiteSpace(altText)
                    ? await client.UploadMedia(imageStream, filename, altText)
                    : await client.UploadMedia(imageStream, filename);

                return (index, attachment);
            }
            catch (Exception ex)
            {
                // If this was our last retry, throw the exception
                if (attempt == maxRetries)
                {
                    throw new Exception($"Failed to upload image {imageUrl} after {maxRetries} attempts: {ex.Message}",
                        ex);
                }

                // Wait before retrying, using exponential backoff
                await Task.Delay(retryDelay);
                retryDelay *= 2; // Double the delay for next attempt
            }
        }

        // This shouldn't be reached due to the throw in the catch block, but compiler needs it
        throw new Exception($"Failed to upload image {imageUrl}");
    }
}

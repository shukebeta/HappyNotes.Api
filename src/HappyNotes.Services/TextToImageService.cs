using CoreHtmlToImage;
using HappyNotes.Services.interfaces;
using Markdig;
using Microsoft.Extensions.Logging;

namespace HappyNotes.Services;

public class TextToImageService(ILogger<TextToImageService> logger) : ITextToImageService
{
    public async Task<byte[]> GenerateImageAsync(
        string content,
        bool isMarkdown = false,
        int width = 600,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Generating image for content. Length: {ContentLength}, IsMarkdown: {IsMarkdown}, Width: {Width}",
            content.Length, isMarkdown, width);

        try
        {
            // Convert markdown to HTML if needed
            var htmlContent = isMarkdown
                ? Markdown.ToHtml(content, new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build())
                : content.Replace("\n", "<br>");

            // Wrap in HTML template with existing CSS
            htmlContent =
                $"<html lang=\"en\">\n<head>\n<meta charset=\"UTF-8\">\n<link rel=\"stylesheet\" href=\"https://files.shukebeta.com/markdown.css?v2\" />\n</head>\n<body>\n{htmlContent}</body></html>";

            // Convert to image using existing library
            var converter = new HtmlConverter();
            var bytes = converter.FromHtmlString(htmlContent, width: width);

            logger.LogDebug("Successfully generated image. Size: {ByteCount} bytes", bytes.Length);

            return await Task.FromResult(bytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate image from content. Length: {ContentLength}, IsMarkdown: {IsMarkdown}",
                content.Length, isMarkdown);
            throw;
        }
    }
}

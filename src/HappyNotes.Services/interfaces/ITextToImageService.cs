namespace HappyNotes.Services.interfaces;

public interface ITextToImageService
{
    /// <summary>
    /// Generate an image from text content (for long messages)
    /// Returns image bytes that can be uploaded to any platform
    /// </summary>
    Task<byte[]> GenerateImageAsync(
        string content,
        bool isMarkdown = false,
        int width = 600,
        CancellationToken cancellationToken = default);
}

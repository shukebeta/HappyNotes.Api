namespace HappyNotes.Common;

/// <summary>
/// Helper class for markdown image transformation operations.
/// Extracted from MastodonTootService for better testability.
/// </summary>
public static class MarkdownImageHelper
{
    /// <summary>
    /// Transforms a failed image markdown into a readable fallback format.
    /// </summary>
    /// <param name="altText">The alt text from the markdown image</param>
    /// <param name="imageUrl">The URL of the failed image</param>
    /// <returns>A readable string in the format "alt: url" or just "url" if alt is "image" or empty</returns>
    public static string TransformFailedImage(string altText, string imageUrl)
    {
        // Remove "image" default alt text (case-insensitive, null-safe)
        if (string.Equals(altText, "image", StringComparison.OrdinalIgnoreCase))
        {
            altText = string.Empty;
        }

        // Return formatted fallback text or just URL
        return !string.IsNullOrWhiteSpace(altText)
            ? $"{altText}: {imageUrl}"
            : imageUrl;
    }

    /// <summary>
    /// Replaces markdown image syntax with fallback text for a failed image upload.
    /// </summary>
    /// <param name="markdownText">The markdown text containing the image</param>
    /// <param name="imageMatch">The original markdown image match string</param>
    /// <param name="altText">The alt text from the image</param>
    /// <param name="imageUrl">The URL of the failed image</param>
    /// <returns>The markdown text with the failed image replaced</returns>
    public static string ReplaceFailedImage(string markdownText, string imageMatch, string altText, string imageUrl)
    {
        var fallbackText = TransformFailedImage(altText, imageUrl);
        return markdownText.Replace(imageMatch, fallbackText);
    }
}

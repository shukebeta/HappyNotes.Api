using HappyNotes.Common;

namespace HappyNotes.Services.Tests;

[TestFixture]
public class MarkdownImageHelperTests
{
    [Test]
    public void TransformFailedImage_WithCustomAltText_ReturnsAltColonUrl()
    {
        // Arrange
        var altText = "My Photo";
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("My Photo: https://example.com/photo.jpg"));
    }

    [Test]
    public void TransformFailedImage_WithDefaultAltTextImage_ReturnsUrlOnly()
    {
        // Arrange
        var altText = "image";
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/photo.jpg"));
    }

    [Test]
    public void TransformFailedImage_WithUppercaseImageAltText_ReturnsUrlOnly()
    {
        // Arrange
        var altText = "Image";
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/photo.jpg"));
    }

    [Test]
    public void TransformFailedImage_WithAllCapsImageAltText_ReturnsUrlOnly()
    {
        // Arrange
        var altText = "IMAGE";
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/photo.jpg"));
    }

    [Test]
    public void TransformFailedImage_WithEmptyAltText_ReturnsUrlOnly()
    {
        // Arrange
        var altText = "";
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/photo.jpg"));
    }

    [Test]
    public void TransformFailedImage_WithNullAltText_ReturnsUrlOnly()
    {
        // Arrange
        string altText = null;
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/photo.jpg"));
    }

    [Test]
    public void TransformFailedImage_WithWhitespaceAltText_ReturnsUrlOnly()
    {
        // Arrange
        var altText = "   ";
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/photo.jpg"));
    }

    [Test]
    public void TransformFailedImage_WithAltTextContainingImageWord_ReturnsAltColonUrl()
    {
        // Arrange
        var altText = "My image description";
        var imageUrl = "https://example.com/photo.jpg";

        // Act
        var result = MarkdownImageHelper.TransformFailedImage(altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("My image description: https://example.com/photo.jpg"));
    }

    [Test]
    public void ReplaceFailedImage_WithSimpleMarkdown_ReplacesCorrectly()
    {
        // Arrange
        var markdownText = "Check this out ![Photo](https://example.com/img.jpg)";
        var imageMatch = "![Photo](https://example.com/img.jpg)";
        var altText = "Photo";
        var imageUrl = "https://example.com/img.jpg";

        // Act
        var result = MarkdownImageHelper.ReplaceFailedImage(markdownText, imageMatch, altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("Check this out Photo: https://example.com/img.jpg"));
    }

    [Test]
    public void ReplaceFailedImage_WithMultipleImages_ReplacesOnlyTarget()
    {
        // Arrange
        var markdownText = "![First](https://example.com/1.jpg) ![Second](https://example.com/2.jpg)";
        var imageMatch = "![First](https://example.com/1.jpg)";
        var altText = "First";
        var imageUrl = "https://example.com/1.jpg";

        // Act
        var result = MarkdownImageHelper.ReplaceFailedImage(markdownText, imageMatch, altText, imageUrl);

        // Assert
        Assert.That(result, Does.StartWith("First: https://example.com/1.jpg"));
        Assert.That(result, Does.Contain("![Second](https://example.com/2.jpg)"));
    }

    [Test]
    public void ReplaceFailedImage_WithDefaultAlt_RemovesAltPrefix()
    {
        // Arrange
        var markdownText = "See this ![image](https://example.com/img.jpg)";
        var imageMatch = "![image](https://example.com/img.jpg)";
        var altText = "image";
        var imageUrl = "https://example.com/img.jpg";

        // Act
        var result = MarkdownImageHelper.ReplaceFailedImage(markdownText, imageMatch, altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("See this https://example.com/img.jpg"));
    }

    [Test]
    public void ReplaceFailedImage_WithUppercaseImageAlt_RemovesAltPrefix()
    {
        // Arrange
        var markdownText = "See this ![Image](https://example.com/img.jpg)";
        var imageMatch = "![Image](https://example.com/img.jpg)";
        var altText = "Image";
        var imageUrl = "https://example.com/img.jpg";

        // Act
        var result = MarkdownImageHelper.ReplaceFailedImage(markdownText, imageMatch, altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("See this https://example.com/img.jpg"));
    }

    [Test]
    public void ReplaceFailedImage_WithLongUrl_PreservesUrl()
    {
        // Arrange
        var markdownText = "![Photo](https://cdn.example.com/very/long/path/to/image.jpg?v=1&token=abc123)";
        var imageMatch = "![Photo](https://cdn.example.com/very/long/path/to/image.jpg?v=1&token=abc123)";
        var altText = "Photo";
        var imageUrl = "https://cdn.example.com/very/long/path/to/image.jpg?v=1&token=abc123";

        // Act
        var result = MarkdownImageHelper.ReplaceFailedImage(markdownText, imageMatch, altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo($"Photo: {imageUrl}"));
    }

    [Test]
    public void ReplaceFailedImage_DoesNotModifyOriginalString_WhenNoMatch()
    {
        // Arrange
        var markdownText = "No image here";
        var imageMatch = "![Photo](https://example.com/img.jpg)";
        var altText = "Photo";
        var imageUrl = "https://example.com/img.jpg";

        // Act
        var result = MarkdownImageHelper.ReplaceFailedImage(markdownText, imageMatch, altText, imageUrl);

        // Assert
        Assert.That(result, Is.EqualTo("No image here"));
    }
}

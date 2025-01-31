using HappyNotes.Common;

namespace HappyNotes.Extensions.Tests;

using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class StringExtensionGetImgInfosTest
{
    [Test]
    public void GetImgInfos_NullOrEmptyInput_ReturnsEmptyList()
    {
        // Arrange
        string? nullInput = null;
        string emptyInput = "";

        // Act & Assert
        Assert.That(nullInput.GetImgInfos(), Is.Empty);
        Assert.That(emptyInput.GetImgInfos(), Is.Empty);
    }

    [Test]
    public void GetImgInfos_NoImages_ReturnsEmptyList()
    {
        // Arrange
        string input = "This is text without images [not an image](url)";

        // Act
        var result = input.GetImgInfos();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetImgInfos_SingleImage_ReturnsCorrectInfo()
    {
        // Arrange
        string input = "![A cat](http://cats.com/cat.jpg)";

        // Act
        var result = input.GetImgInfos();

        // Assert
        Assert.That(result, Has.Exactly(1).Items);
        Assert.That(result[0].Alt, Is.EqualTo("A cat"));
        Assert.That(result[0].ImgUrl, Is.EqualTo("http://cats.com/cat.jpg"));
    }

    [Test]
    public void GetImgInfos_MultipleImages_ReturnsAllInfos()
    {
        // Arrange
        string input = "![First](img1.png) Text between ![Second](img2.jpg)";

        // Act
        var result = input.GetImgInfos();

        // Assert
        var expected = new List<(string, string)> {
            ("First", "img1.png"),
            ("Second", "img2.jpg")
        };

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetImgInfos_TrimsWhitespace()
    {
        // Arrange
        string input = "![   spaced alt  ](  http://url.com/image.png  )";

        // Act
        var result = input.GetImgInfos();

        // Assert
        Assert.That(result[0].Alt, Is.EqualTo("spaced alt"));
        Assert.That(result[0].ImgUrl, Is.EqualTo("http://url.com/image.png"));
    }

    [Test]
    public void GetImgInfos_IgnoresMalformedMarkdown()
    {
        // Arrange
        string input = "![Missing closing bracket](url.png [Missing exclamation](";

        // Act
        var result = input.GetImgInfos();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TestCase("![](url.png)", "", "url.png")] // Empty alt text
    [TestCase("![Alt]()", "Alt", "")]         // Empty URL
    public void GetImgInfos_HandlesEdgeCases(string input, string expectedAlt, string expectedUrl)
    {
        // Act
        var result = input.GetImgInfos();

        // Assert
        Assert.That(result[0].Alt, Is.EqualTo(expectedAlt));
        Assert.That(result[0].ImgUrl, Is.EqualTo(expectedUrl));
    }
}

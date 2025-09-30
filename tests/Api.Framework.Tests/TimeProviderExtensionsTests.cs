using Api.Framework.Extensions;
using Moq;

namespace Api.Framework.Tests;

[TestFixture]
public class TimeProviderExtensionsTests
{
    [Test]
    public void GetUtcNowUnixTimeSeconds_ReturnsCorrectTimestamp()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2024, 3, 15, 10, 30, 45, TimeSpan.Zero);
        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(tp => tp.GetUtcNow()).Returns(fixedTime);

        // Act
        var result = mockTimeProvider.Object.GetUtcNowUnixTimeSeconds();

        // Assert
        var expected = fixedTime.ToUnixTimeSeconds();
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetLocalNowUnixTimeSeconds_WithSystemTimeProvider_ReturnsReasonableValue()
    {
        // Arrange
        var systemTimeProvider = TimeProvider.System;
        var beforeCall = DateTime.Now.ToUnixTimeSeconds();

        // Act
        var result = systemTimeProvider.GetLocalNowUnixTimeSeconds();
        var afterCall = DateTime.Now.ToUnixTimeSeconds();

        // Assert - result should be between before and after call
        Assert.That(result, Is.GreaterThanOrEqualTo(beforeCall));
        Assert.That(result, Is.LessThanOrEqualTo(afterCall));
    }

    [Test]
    public void GetUtcNowUnixTimeSeconds_WithSystemTimeProvider_ReturnsReasonableValue()
    {
        // Arrange
        var systemTimeProvider = TimeProvider.System;
        var beforeCall = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Act
        var result = systemTimeProvider.GetUtcNowUnixTimeSeconds();
        var afterCall = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Assert - result should be between before and after call
        Assert.That(result, Is.GreaterThanOrEqualTo(beforeCall));
        Assert.That(result, Is.LessThanOrEqualTo(afterCall));
    }

    [TestCase(0, "1970-01-01 00:00:00")]
    [TestCase(1717665363, "2024-06-06 09:16:03")] // Known timestamp
    public void GetUtcNowUnixTimeSeconds_SpecificTimestamps_MatchesExpected(long expectedTimestamp, string dateString)
    {
        // Arrange
        var fixedTime = DateTimeOffset.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.AssumeUniversal);
        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(tp => tp.GetUtcNow()).Returns(fixedTime);

        // Act
        var result = mockTimeProvider.Object.GetUtcNowUnixTimeSeconds();

        // Assert
        Assert.That(result, Is.EqualTo(expectedTimestamp));
    }
}
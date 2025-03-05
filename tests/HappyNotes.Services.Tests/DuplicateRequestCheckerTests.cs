using HappyNotes.Models;

namespace HappyNotes.Services.Tests;

[TestFixture]
public class DuplicateRequestCheckerTests
{
    [SetUp]
    public void Setup()
    {
        // Set a smaller cleanup interval for testing
        DuplicateRequestChecker.SetCleanupInterval(TimeSpan.FromSeconds(3));
        DuplicateRequestChecker.SetExpirationDuration(TimeSpan.FromSeconds(2));
        DuplicateRequestChecker.RecentRequests.Clear();
    }

    [Test]
    public void IsDuplicate_ShouldReturnFalse_WhenRequestIsUnique()
    {
        // Arrange
        var userId = 1;
        var request = new PostNoteRequest { Content = "Unique content", };

        // Act
        var isDuplicate = DuplicateRequestChecker.IsDuplicate(userId, request);

        // Assert
        Assert.IsFalse(isDuplicate);
    }

    [Test]
    public void IsDuplicate_ShouldReturnTrue_WhenDuplicateRequestWithinThreshold()
    {
        // Arrange
        var userId = 2;
        var request = new PostNoteRequest { Content = "Duplicate content", };

        // Act
        DuplicateRequestChecker.IsDuplicate(userId, request);
        var isDuplicate = DuplicateRequestChecker.IsDuplicate(userId, request);

        // Assert
        Assert.IsTrue(isDuplicate);
    }

    [Test]
    public async Task PeriodicCleanup_ShouldRemoveOldEntries()
    {
        // Arrange
        await Task.Delay(TimeSpan.FromSeconds(3)); // Wait for cleanup

        var userId = 3;
        var request = new PostNoteRequest { Content = "Old entry content", };

        // Add an old entry
        DuplicateRequestChecker.IsDuplicate(userId, request);
        Assert.That(DuplicateRequestChecker.Length(3), Is.EqualTo(1));

        // Act
        await Task.Delay(TimeSpan.FromSeconds(3)); // Wait for cleanup

        Assert.That(DuplicateRequestChecker.Length(3), Is.EqualTo(0));
    }

    [Test]
    public void StopBackgroundTask_ShouldStopCleanupTask()
    {
        // Act
        DuplicateRequestChecker.StopBackgroundTask();

        // Assert
        // No direct assertion possible; ensure no exceptions are thrown
    }
}

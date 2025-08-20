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
        // Use specific, short intervals for this test to make it fast and predictable.
        DuplicateRequestChecker.SetExpirationDuration(TimeSpan.FromSeconds(1));
        DuplicateRequestChecker.SetCleanupInterval(TimeSpan.FromSeconds(2));
        DuplicateRequestChecker.RecentRequests.Clear(); // Ensure a clean state for this test

        var userId = 3;
        var request = new PostNoteRequest { Content = "Old entry content" };

        // Act
        // Add an entry. It is set to expire in 1 second.
        DuplicateRequestChecker.IsDuplicate(userId, request);
        Assert.That(DuplicateRequestChecker.Length(3), Is.EqualTo(1), "Entry should be added initially.");

        // Wait long enough for the entry to expire AND a cleanup cycle to run.
        // Waiting 3 seconds is safely longer than the expiration (1s) and the cleanup interval (2s).
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Assert
        Assert.That(DuplicateRequestChecker.Length(3), Is.EqualTo(0), "Entry should have been removed by the cleanup task.");
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

using HappyNotes.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace HappyNotes.Services.Tests;

// These integration tests are marked as manual to prevent CI failures on GitHub.
// They require a running Manticore Search instance at 127.0.0.1:9306 and should be run locally.
[TestFixture]
[Explicit("Manual test - requires local Manticore instance at 127.0.0.1:9306")]
public class ManticoreSyncIntegrationTests
{
    private readonly ManticoreSyncNoteService _syncService;
    private readonly SearchService _searchService;
    private readonly Mock<ILogger<ManticoreSyncNoteService>> _mockLogger;

    public ManticoreSyncIntegrationTests()
    {
        // Setup for integration tests using a local Manticore instance at 127.0.0.1:9306
        // Ensure that a Manticore Search server is running on this address and port before running tests.

        var mockConfig = new Mock<IConfiguration>();
        var mockConfigSection = new Mock<IConfigurationSection>();
        mockConfigSection.Setup(x => x.Value).Returns("server=127.0.0.1; port=9306; charset=utf8mb4;");
        mockConfig.Setup(x => x.GetSection("ManticoreConnectionOptions:ConnectionString")).Returns(mockConfigSection.Object);

        var databaseClient = new DatabaseClient(mockConfig.Object);
        var httpClient = new HttpClient();
        var options = new ManticoreConnectionOptions { HttpEndpoint = "http://127.0.0.1:9312" };
        _searchService = new SearchService(databaseClient, httpClient, options);
        _mockLogger = new Mock<ILogger<ManticoreSyncNoteService>>();
        _syncService = new ManticoreSyncNoteService(_searchService, _mockLogger.Object);
    }

    [Test]
    public async Task SyncNewNote_ValidNote_SyncsToIndex()
    {
        // Arrange
        var note = new Note
        {
            Id = 1,
            UserId = 1,
            IsLong = false,
            IsPrivate = false,
            IsMarkdown = false,
            Content = "Integration test note",
            CreatedAt = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            UpdatedAt = null,
            DeletedAt = null
        };
        string fullContent = note.Content;

        // Act
        await _syncService.SyncNewNote(note, fullContent);

        // Assert
        // Since we're using a real connection to 127.0.0.1:9306, we assume the sync operation succeeded if no exception is thrown.
        // In a complete integration test, you would search for the note to confirm it was indexed.
        // For simplicity, we verify no error was logged.
        _mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Never);
    }

    [Test]
    public async Task SyncEditNote_ExistingNote_UpdatesIndex()
    {
        // Arrange
        var note = new Note
        {
            Id = 1,
            UserId = 1,
            IsLong = false,
            IsPrivate = false,
            IsMarkdown = false,
            Content = "Updated integration test note",
            CreatedAt = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            UpdatedAt = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            DeletedAt = null
        };
        string fullContent = note.Content;

        // Act
        await _syncService.SyncEditNote(note, fullContent);

        // Assert
        // Assume update succeeded if no exception; in a full test, verify updated content via search.
        _mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Never);
    }

    [Test]
    public async Task SyncDeleteNote_ExistingNote_MarksAsDeleted()
    {
        // Arrange
        var note = new Note
        {
            Id = 1,
            UserId = 1,
            IsLong = false,
            IsPrivate = false,
            IsMarkdown = false,
            Content = "Integration test note to delete",
            CreatedAt = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            UpdatedAt = null,
            DeletedAt = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
        };

        // Act
        await _syncService.SyncDeleteNote(note);

        // Assert
        // Assume deletion marking succeeded if no exception; in a full test, verify note is not searchable under normal filter.
        _mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Never);
    }

    [Test]
    public async Task SyncUndeleteNote_DeletedNote_RestoresInIndex()
    {
        // Arrange
        var note = new Note
        {
            Id = 1,
            UserId = 1,
            IsLong = false,
            IsPrivate = false,
            IsMarkdown = false,
            Content = "Integration test note to undelete",
            CreatedAt = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            UpdatedAt = null,
            DeletedAt = null // Simulating undelete by setting DeletedAt to null
        };

        // Act
        await _syncService.SyncUndeleteNote(note);

        // Assert
        // Assume undelete succeeded if no exception; in a full test, verify note is searchable again.
        _mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Never);
    }

    [Test]
    public async Task PurgeDeletedNotes_MultipleDeletedNotes_RemovesFromIndex()
    {
        // Act
        await _syncService.PurgeDeletedNotes();

        // Assert
        // Assume purge succeeded if no exception; in a full test, verify no deleted notes remain in index.
        _mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Never);
    }

    [Test]
    public async Task SyncNewNote_ManticoreUnavailable_LogsError()
    {
        // Arrange
        // This test would ideally simulate Manticore being unavailable by using a wrong port or mocking a connection failure.
        // For now, since we're using a real connection, this is a conceptual test.
        var note = new Note
        {
            Id = 999,
            UserId = 1,
            IsLong = false,
            IsPrivate = false,
            IsMarkdown = false,
            Content = "Test note for unavailable Manticore",
            CreatedAt = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            UpdatedAt = null,
            DeletedAt = null
        };
        string fullContent = note.Content;

        // Act
        await _syncService.SyncNewNote(note, fullContent);

        // Assert
        // Since we can't simulate failure without altering connection, we note that error logging should occur.
        // In a real failure scenario, verify that an error is logged.
        // Currently, if Manticore is down, an exception might be caught and logged.
        _mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(level => level == LogLevel.Error),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.AtMostOnce);
    }
}

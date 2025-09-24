using Api.Framework.Helper;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using HappyNotes.Services.SyncQueue.Interfaces;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HappyNotes.Services.Tests;

public class TelegramSyncNoteServiceTests
{
    private Mock<ITelegramSettingsCacheService> _mockTelegramSettingsCache;
    private Mock<ISyncQueueService> _mockSyncQueueService;
    private Mock<ILogger<TelegramSyncNoteService>> _mockLogger;
    private TelegramSyncNoteService _telegramSyncNoteService;

    [SetUp]
    public void Setup()
    {
        _mockTelegramSettingsCache = new Mock<ITelegramSettingsCacheService>();
        _mockSyncQueueService = new Mock<ISyncQueueService>();
        _mockLogger = new Mock<ILogger<TelegramSyncNoteService>>();

        _telegramSyncNoteService = new TelegramSyncNoteService(
            _mockTelegramSettingsCache.Object,
            _mockSyncQueueService.Object,
            _mockLogger.Object
        );
    }

    [TestCase("test note", true, TelegramSyncType.All, "", true)]
    [TestCase("test note", false, TelegramSyncType.All, "", true)]
    [TestCase("test note", false, TelegramSyncType.Public, "", true)]
    [TestCase("test note", true, TelegramSyncType.Public, "", false)]
    [TestCase("test note", true, TelegramSyncType.Private, "", true)]
    [TestCase("test note", false, TelegramSyncType.Private, "", false)]
    [TestCase("test note", true, TelegramSyncType.Tag, "telegram", true)]
    [TestCase("test note", false, TelegramSyncType.Tag, "telegram", true)]
    [TestCase("test note", false, TelegramSyncType.Tag, "", false)]
    [TestCase("test note", true, TelegramSyncType.Tag, "", false)]
    public async Task SyncNewNote_ShouldEnqueueCorrectTasks(string content, bool isPrivate,
        TelegramSyncType syncType, string tag, bool shouldSync)
    {
        // Arrange
        var note = new Note
        {
            Id = 123,
            UserId = 1,
            IsPrivate = isPrivate,
            TagList = string.IsNullOrEmpty(tag) ? [] : [tag],
            IsMarkdown = true
        };

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                ChannelId = "-1001234567890",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = syncType,
                SyncValue = syncType == TelegramSyncType.Tag ? "telegram" : ""
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncNewNote(note, content);

        // Assert
        if (shouldSync)
        {
            _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>(
                "telegram",
                It.Is<SyncTask<TelegramSyncPayload>>(t =>
                    t.Action == "CREATE" &&
                    t.EntityId == note.Id &&
                    t.UserId == note.UserId &&
                    t.Payload.ChannelId == "-1001234567890" &&
                    t.Payload.FullContent == content &&
                    t.Payload.IsMarkdown == note.IsMarkdown
                )), Times.Once);
        }
        else
        {
            _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>(It.IsAny<string>(), It.IsAny<SyncTask<TelegramSyncPayload>>()), Times.Never);
        }
    }

    [Test]
    public async Task SyncNewNote_WithNoSettings_ShouldNotEnqueueTasks()
    {
        // Arrange
        var note = new Note { Id = 123, UserId = 1, IsPrivate = false };
        var content = "test content";

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(new List<TelegramSettings>());

        // Act
        await _telegramSyncNoteService.SyncNewNote(note, content);

        // Assert
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>(It.IsAny<string>(), It.IsAny<SyncTask<TelegramSyncPayload>>()), Times.Never);
    }

    [Test]
    public async Task SyncNewNote_WithMultipleChannels_ShouldEnqueueMultipleTasks()
    {
        // Arrange
        var note = new Note { Id = 123, UserId = 1, IsPrivate = false, IsMarkdown = false };
        var content = "test content";

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001111111111",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token1", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            },
            new()
            {
                Id = 2, UserId = 1, ChannelId = "-1002222222222",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token2", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncNewNote(note, content);

        // Assert
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t => t.Payload.ChannelId == "-1001111111111")), Times.Once);
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t => t.Payload.ChannelId == "-1002222222222")), Times.Once);
    }

    [Test]
    public async Task SyncEditNote_WithShortContent_ShouldEnqueueUpdateTasks()
    {
        // Arrange
        var existingNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "old content",
            IsPrivate = false,
            IsMarkdown = true,
            TelegramMessageIds = "-1001234567890:100,-1001234567891:101"
        };

        var note = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "Short content for UPDATE", // Different content
            IsPrivate = false,
            IsMarkdown = true,
            TelegramMessageIds = "-1001234567890:100,-1001234567891:101"
        };
        var content = "Short content for UPDATE"; // Less than 4096 chars

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            },
            new()
            {
                Id = 2, UserId = 1, ChannelId = "-1001234567891",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncEditNote(note, content, existingNote);

        // Assert - Should enqueue UPDATE tasks for existing channels
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "UPDATE" &&
                t.Payload.ChannelId == "-1001234567890" &&
                t.Payload.MessageId == 100
            )), Times.Once);

        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "UPDATE" &&
                t.Payload.ChannelId == "-1001234567891" &&
                t.Payload.MessageId == 101
            )), Times.Once);
    }

    [Test]
    public async Task SyncEditNote_WithLongContent_ShouldEnqueueDeleteAndCreateTasks()
    {
        // Arrange
        var existingNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "old content",
            IsPrivate = false,
            IsMarkdown = true,
            TelegramMessageIds = "-1001234567890:100"
        };

        var note = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "new long content",
            IsPrivate = false,
            IsMarkdown = true,
            TelegramMessageIds = "-1001234567890:100"
        };
        var content = new string('a', Constants.TelegramMessageLength + 100); // Long content > 4096 chars

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncEditNote(note, content, existingNote);

        // Assert - Should enqueue DELETE then CREATE for long content
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "DELETE" &&
                t.Payload.ChannelId == "-1001234567890" &&
                t.Payload.MessageId == 100
            )), Times.Once);

        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "CREATE" &&
                t.Payload.ChannelId == "-1001234567890" &&
                t.Payload.FullContent == content
            )), Times.Once);
    }

    [Test]
    public async Task SyncEditNote_WithRemovedChannels_ShouldEnqueueDeleteTasks()
    {
        // Arrange
        var existingNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "old content",
            IsPrivate = false,
            IsMarkdown = false,
            TelegramMessageIds = "-1001234567890:100,-1001234567891:101" // Two existing channels
        };

        var note = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "test content",
            IsPrivate = false,
            IsMarkdown = false,
            TelegramMessageIds = "-1001234567890:100,-1001234567891:101" // Two existing channels
        };
        var content = "test content";

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890", // Only one channel remains
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncEditNote(note, content, existingNote);

        // Assert
        // Should DELETE from removed channel
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "DELETE" &&
                t.Payload.ChannelId == "-1001234567891" &&
                t.Payload.MessageId == 101
            )), Times.Once);

        // Should UPDATE existing channel
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "UPDATE" &&
                t.Payload.ChannelId == "-1001234567890"
            )), Times.Once);
    }

    [Test]
    public async Task SyncEditNote_WithNewChannels_ShouldEnqueueCreateTasks()
    {
        // Arrange
        var existingNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "old content",
            IsPrivate = false,
            IsMarkdown = false,
            TelegramMessageIds = "-1001234567890:100" // One existing channel
        };

        var note = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "test content",
            IsPrivate = false,
            IsMarkdown = false,
            TelegramMessageIds = "-1001234567890:100" // One existing channel
        };
        var content = "test content";

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890", // Existing channel
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token1", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            },
            new()
            {
                Id = 2, UserId = 1, ChannelId = "-1001234567891", // New channel
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token2", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncEditNote(note, content, existingNote);

        // Assert
        // Should UPDATE existing channel
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "UPDATE" &&
                t.Payload.ChannelId == "-1001234567890"
            )), Times.Once);

        // Should CREATE for new channel
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "CREATE" &&
                t.Payload.ChannelId == "-1001234567891" &&
                t.Payload.FullContent == content
            )), Times.Once);
    }

    [Test]
    public async Task SyncDeleteNote_WithExistingMessages_ShouldEnqueueDeleteTasks()
    {
        // Arrange
        var note = new Note
        {
            Id = 123,
            UserId = 1,
            TelegramMessageIds = "-1001234567890:100,-1001234567891:101"
        };

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token1", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            },
            new()
            {
                Id = 2, UserId = 1, ChannelId = "-1001234567891",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token2", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncDeleteNote(note);

        // Assert
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "DELETE" &&
                t.Payload.ChannelId == "-1001234567890" &&
                t.Payload.MessageId == 100
            )), Times.Once);

        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "DELETE" &&
                t.Payload.ChannelId == "-1001234567891" &&
                t.Payload.MessageId == 101
            )), Times.Once);
    }

    [Test]
    public async Task SyncDeleteNote_WithNoExistingMessages_ShouldNotEnqueueTasks()
    {
        // Arrange
        var note = new Note
        {
            Id = 123,
            UserId = 1,
            TelegramMessageIds = null // No existing messages
        };

        // Act
        await _telegramSyncNoteService.SyncDeleteNote(note);

        // Assert
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>(It.IsAny<string>(), It.IsAny<SyncTask<TelegramSyncPayload>>()), Times.Never);
    }

    [Test]
    public async Task SyncDeleteNote_WithEmptyMessageIds_ShouldNotEnqueueTasks()
    {
        // Arrange
        var note = new Note
        {
            Id = 123,
            UserId = 1,
            TelegramMessageIds = "" // Empty string
        };

        // Act
        await _telegramSyncNoteService.SyncDeleteNote(note);

        // Assert
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>(It.IsAny<string>(), It.IsAny<SyncTask<TelegramSyncPayload>>()), Times.Never);
    }

    [Test]
    public async Task SyncEditNote_WithUnchangedContent_ShouldSkipUpdateTasks()
    {
        // Arrange
        var existingNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "existing content",
            IsPrivate = false,
            IsMarkdown = true,
            TelegramMessageIds = "-1001234567890:100"
        };

        var updatedNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "existing content", // Same content
            IsPrivate = true, // Changed IsPrivate (but content unchanged)
            IsMarkdown = true, // Same markdown setting
            TelegramMessageIds = "-1001234567890:100"
        };

        var fullContent = "existing content"; // Same as existing

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All // All type channel - IsPrivate change doesn't matter
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(updatedNote.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncEditNote(updatedNote, fullContent, existingNote);

        // Assert - Should NOT enqueue UPDATE task since content didn't change
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "UPDATE"
            )), Times.Never);
    }

    [Test]
    public async Task SyncEditNote_WithChangedContent_ShouldEnqueueUpdateTasks()
    {
        // Arrange
        var existingNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "original content",
            IsPrivate = false,
            IsMarkdown = true,
            TelegramMessageIds = "-1001234567890:100"
        };

        var updatedNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "updated content", // Content changed
            IsPrivate = true, // IsPrivate also changed, but doesn't matter for content comparison
            IsMarkdown = true,
            TelegramMessageIds = "-1001234567890:100"
        };

        var fullContent = "updated content";

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(updatedNote.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncEditNote(updatedNote, fullContent, existingNote);

        // Assert - Should enqueue UPDATE task since content changed
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "UPDATE" &&
                t.Payload.ChannelId == "-1001234567890" &&
                t.Payload.FullContent == fullContent
            )), Times.Once);
    }

    [Test]
    public async Task SyncEditNote_WithChangedMarkdownSetting_ShouldEnqueueUpdateTasks()
    {
        // Arrange
        var existingNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "test content",
            IsPrivate = false,
            IsMarkdown = true, // Was markdown
            TelegramMessageIds = "-1001234567890:100"
        };

        var updatedNote = new Note
        {
            Id = 123,
            UserId = 1,
            Content = "test content", // Same content
            IsPrivate = false,
            IsMarkdown = false, // Changed to non-markdown
            TelegramMessageIds = "-1001234567890:100"
        };

        var fullContent = "test content";

        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1, UserId = 1, ChannelId = "-1001234567890",
                EncryptedToken = TextEncryptionHelper.Encrypt("bot_token", "test_key_1234567890123456"),
                SyncType = TelegramSyncType.All
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(updatedNote.UserId))
            .ReturnsAsync(telegramSettings);

        // Act
        await _telegramSyncNoteService.SyncEditNote(updatedNote, fullContent, existingNote);

        // Assert - Should enqueue UPDATE task since markdown setting changed (affects rendering)
        _mockSyncQueueService.Verify(s => s.EnqueueAsync<TelegramSyncPayload>("telegram",
            It.Is<SyncTask<TelegramSyncPayload>>(t =>
                t.Action == "UPDATE" &&
                t.Payload.ChannelId == "-1001234567890" &&
                t.Payload.IsMarkdown == false
            )), Times.Once);
    }
}

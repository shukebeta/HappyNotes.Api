using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using HappyNotes.Services.SyncQueue.Interfaces;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyNotes.Services.Tests;

public class FanfouSyncNoteServiceTests
{
    private Mock<INoteRepository> _mockNoteRepository;
    private Mock<IFanfouUserAccountCacheService> _mockFanfouUserAccountCacheService;
    private Mock<ISyncQueueService> _mockSyncQueueService;
    private Mock<IOptions<JwtConfig>> _mockJwtConfig;
    private Mock<ILogger<FanfouSyncNoteService>> _mockLogger;
    private FanfouSyncNoteService _fanfouSyncNoteService;

    [SetUp]
    public void Setup()
    {
        _mockNoteRepository = new Mock<INoteRepository>();
        _mockFanfouUserAccountCacheService = new Mock<IFanfouUserAccountCacheService>();
        _mockSyncQueueService = new Mock<ISyncQueueService>();
        _mockJwtConfig = new Mock<IOptions<JwtConfig>>();
        _mockLogger = new Mock<ILogger<FanfouSyncNoteService>>();

        var jwtConfig = new JwtConfig { SymmetricSecurityKey = "test_key" };
        _mockJwtConfig.Setup(x => x.Value).Returns(jwtConfig);

        _fanfouSyncNoteService = new FanfouSyncNoteService(
            _mockNoteRepository.Object,
            _mockFanfouUserAccountCacheService.Object,
            _mockJwtConfig.Object,
            _mockSyncQueueService.Object,
            _mockLogger.Object
        );
    }

    [TestCase("test note", true, FanfouSyncType.All, "", true)]
    [TestCase("test note", false, FanfouSyncType.All, "", true)]
    [TestCase("test note", false, FanfouSyncType.PublicOnly, "", true)]
    [TestCase("test note", true, FanfouSyncType.PublicOnly, "", false)]
    [TestCase("test note", true, FanfouSyncType.TagFanfouOnly, "fanfou", true)]
    [TestCase("test note", false, FanfouSyncType.TagFanfouOnly, "fanfou", true)]
    [TestCase("test note", false, FanfouSyncType.TagFanfouOnly, "", false)]
    [TestCase("test note", true, FanfouSyncType.TagFanfouOnly, "", false)]
    public async Task SyncNewNote_ShouldSyncNoteToFanfou(string fullContent, bool isPrivate,
        FanfouSyncType syncType, string tag, bool shouldSync)
    {
        // Arrange
        var note = new Note
        {
            UserId = 1,
            IsPrivate = isPrivate,
            TagList = [tag,]
        };

        var fanfouUserAccounts = new List<FanfouUserAccount>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Username = "testuser",
                ConsumerKey = TextEncryptionHelper.Encrypt("test_key", "test_key"),
                ConsumerSecret = TextEncryptionHelper.Encrypt("test_secret", "test_key"),
                AccessToken = TextEncryptionHelper.Encrypt("test_token", "test_key"),
                AccessTokenSecret = TextEncryptionHelper.Encrypt("test_token_secret", "test_key"),
                SyncType = syncType
            }
        };

        _mockFanfouUserAccountCacheService
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(fanfouUserAccounts);

        // Act
        await _fanfouSyncNoteService.SyncNewNote(note, fullContent);

        // Assert - Verify queue operation
        if (shouldSync)
        {
            _mockSyncQueueService.Verify(s => s.EnqueueAsync("fanfou",
                It.IsAny<SyncTask<FanfouSyncPayload>>()),
                Times.Once);
        }
        else
        {
            _mockSyncQueueService.Verify(s => s.EnqueueAsync(It.IsAny<string>(), It.IsAny<SyncTask<FanfouSyncPayload>>()), Times.Never);
        }
    }

    [TestCase("Short note", false, true)]
    [TestCase("A very long note that exceeds 140 characters limit for Fanfou status update and requires image generation to preserve the full content", true, true)]
    public async Task SyncNewNote_ShouldEnqueueWithOriginalContent(string fullContent, bool isLong, bool shouldSync)
    {
        // Arrange
        var note = new Note
        {
            Id = 100,
            UserId = 1,
            IsPrivate = false,
            TagList = []
        };

        var fanfouUserAccounts = new List<FanfouUserAccount>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Username = "testuser",
                ConsumerKey = TextEncryptionHelper.Encrypt("test_key", "test_key"),
                ConsumerSecret = TextEncryptionHelper.Encrypt("test_secret", "test_key"),
                AccessToken = TextEncryptionHelper.Encrypt("test_token", "test_key"),
                AccessTokenSecret = TextEncryptionHelper.Encrypt("test_token_secret", "test_key"),
                SyncType = FanfouSyncType.All
            }
        };

        _mockFanfouUserAccountCacheService
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(fanfouUserAccounts);

        // Act
        await _fanfouSyncNoteService.SyncNewNote(note, fullContent);

        // Assert - The original content should be enqueued
        _mockSyncQueueService.Verify(s => s.EnqueueAsync("fanfou",
            It.IsAny<SyncTask<FanfouSyncPayload>>()),
            shouldSync ? Times.Once : Times.Never);
    }

    [Test]
    public async Task SyncDeleteNote_WithFanfouStatusIds_ShouldEnqueueDeleteTasks()
    {
        // Arrange
        var note = new Note
        {
            Id = 123,
            UserId = 1,
            FanfouStatusIds = "1:status123,2:status456"
        };

        var fanfouUserAccounts = new List<FanfouUserAccount>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Username = "testuser",
                ConsumerKey = TextEncryptionHelper.Encrypt("test_key", "test_key"),
                ConsumerSecret = TextEncryptionHelper.Encrypt("test_secret", "test_key"),
                AccessToken = TextEncryptionHelper.Encrypt("test_token", "test_key"),
                AccessTokenSecret = TextEncryptionHelper.Encrypt("test_token_secret", "test_key"),
                SyncType = FanfouSyncType.All
            },
            new()
            {
                Id = 2,
                UserId = 1,
                Username = "testuser2",
                ConsumerKey = TextEncryptionHelper.Encrypt("test_key2", "test_key"),
                ConsumerSecret = TextEncryptionHelper.Encrypt("test_secret2", "test_key"),
                AccessToken = TextEncryptionHelper.Encrypt("test_token2", "test_key"),
                AccessTokenSecret = TextEncryptionHelper.Encrypt("test_token_secret2", "test_key"),
                SyncType = FanfouSyncType.All
            }
        };

        _mockFanfouUserAccountCacheService
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(fanfouUserAccounts);

        // Act
        await _fanfouSyncNoteService.SyncDeleteNote(note);

        // Assert - Should enqueue DELETE tasks for both statuses
        _mockSyncQueueService.Verify(s => s.EnqueueAsync("fanfou",
            It.IsAny<SyncTask<FanfouSyncPayload>>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task SyncDeleteNote_WithoutFanfouStatusIds_ShouldNotEnqueue()
    {
        // Arrange
        var note = new Note
        {
            Id = 123,
            UserId = 1,
            FanfouStatusIds = null
        };

        var fanfouUserAccounts = new List<FanfouUserAccount>();

        _mockFanfouUserAccountCacheService
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(fanfouUserAccounts);

        // Act
        await _fanfouSyncNoteService.SyncDeleteNote(note);

        // Assert - Should not enqueue any DELETE tasks
        _mockSyncQueueService.Verify(s => s.EnqueueAsync(It.IsAny<string>(), It.IsAny<SyncTask<FanfouSyncPayload>>()), Times.Never);
    }
}

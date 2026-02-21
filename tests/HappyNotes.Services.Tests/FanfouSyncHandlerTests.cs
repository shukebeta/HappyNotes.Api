using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Entities;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using HappyNotes.Services.SyncQueue.Configuration;
using HappyNotes.Services.SyncQueue.Handlers;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyNotes.Services.Tests;

public class FanfouSyncHandlerTests
{
    private Mock<IFanfouStatusService> _mockFanfouStatusService;
    private Mock<IFanfouUserAccountCacheService> _mockFanfouUserAccountCacheService;
    private Mock<ITextToImageService> _mockTextToImageService;
    private Mock<INoteRepository> _mockNoteRepository;
    private Mock<IOptions<SyncQueueOptions>> _mockSyncQueueOptions;
    private Mock<IOptions<JwtConfig>> _mockJwtConfig;
    private Mock<ILogger<FanfouSyncHandler>> _mockLogger;
    private FanfouSyncHandler _fanfouSyncHandler;

    private const string TestConsumerKey = "test_consumer_key";
    private const string TestConsumerSecret = "test_consumer_secret";
    private const string TestAccessToken = "test_access_token";
    private const string TestAccessTokenSecret = "test_access_token_secret";
    private const string TestJwtKey = "test_key_1234567890123456";

    [SetUp]
    public void Setup()
    {
        _mockFanfouStatusService = new Mock<IFanfouStatusService>();
        _mockFanfouUserAccountCacheService = new Mock<IFanfouUserAccountCacheService>();
        _mockTextToImageService = new Mock<ITextToImageService>();
        _mockNoteRepository = new Mock<INoteRepository>();
        _mockSyncQueueOptions = new Mock<IOptions<SyncQueueOptions>>();
        _mockJwtConfig = new Mock<IOptions<JwtConfig>>();
        _mockLogger = new Mock<ILogger<FanfouSyncHandler>>();

        var syncQueueOptions = new SyncQueueOptions();
        _mockSyncQueueOptions.Setup(x => x.Value).Returns(syncQueueOptions);

        var jwtConfig = new JwtConfig { SymmetricSecurityKey = TestJwtKey };
        _mockJwtConfig.Setup(x => x.Value).Returns(jwtConfig);

        _fanfouSyncHandler = new FanfouSyncHandler(
            _mockFanfouStatusService.Object,
            _mockFanfouUserAccountCacheService.Object,
            _mockTextToImageService.Object,
            _mockNoteRepository.Object,
            _mockSyncQueueOptions.Object,
            _mockJwtConfig.Object,
            _mockLogger.Object
        );
    }

    private SyncTask CreateSyncTask(string action, FanfouSyncPayload payload, long noteId = 123, long userId = 1)
    {
        var task = SyncTask.Create("fanfou", action, noteId, userId, payload);
        return new SyncTask
        {
            Id = task.Id,
            Service = task.Service,
            Action = task.Action,
            EntityId = task.EntityId,
            UserId = task.UserId,
            Payload = task.Payload,
            AttemptCount = task.AttemptCount,
            CreatedAt = task.CreatedAt,
            ScheduledFor = task.ScheduledFor,
            Metadata = task.Metadata
        };
    }

    private void SetupFanfouUserAccount(long userId, long userAccountId)
    {
        var fanfouUserAccounts = new List<FanfouUserAccount>
        {
            new()
            {
                Id = userAccountId,
                UserId = userId,
                Username = "testuser",
                ConsumerKey = TextEncryptionHelper.Encrypt(TestConsumerKey, TestJwtKey),
                ConsumerSecret = TextEncryptionHelper.Encrypt(TestConsumerSecret, TestJwtKey),
                AccessToken = TextEncryptionHelper.Encrypt(TestAccessToken, TestJwtKey),
                AccessTokenSecret = TextEncryptionHelper.Encrypt(TestAccessTokenSecret, TestJwtKey)
            }
        };

        _mockFanfouUserAccountCacheService
            .Setup(s => s.GetAsync(userId))
            .ReturnsAsync(fanfouUserAccounts);
    }

    #region CREATE Action Tests

    [Test]
    public async Task ProcessCreateAction_WithShortContent_ShouldSendStatus()
    {
        // Arrange
        var payload = new FanfouSyncPayload
        {
            FullContent = "Short test message",
            UserAccountId = 1,
            IsMarkdown = false
        };

        var task = CreateSyncTask("CREATE", payload);

        SetupFanfouUserAccount(1, 1);
        _mockFanfouStatusService
            .Setup(s => s.SendStatusAsync(TestConsumerKey, TestConsumerSecret, TestAccessToken, TestAccessTokenSecret, payload.FullContent, It.IsAny<CancellationToken>()))
            .ReturnsAsync("status123");

        var testNote = new Note { Id = 123, FanfouStatusIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _fanfouSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockFanfouStatusService.Verify(s => s.SendStatusAsync(TestConsumerKey, TestConsumerSecret, TestAccessToken, TestAccessTokenSecret, payload.FullContent, It.IsAny<CancellationToken>()), Times.Once);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, Note>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
    }

    [Test]
    public async Task ProcessCreateAction_WithLongContent_ShouldGenerateImageAndSendPhoto()
    {
        // Arrange
        var longContent = new string('a', 200); // > 140 chars
        var payload = new FanfouSyncPayload
        {
            FullContent = longContent,
            UserAccountId = 1,
            IsMarkdown = false
        };

        var task = CreateSyncTask("CREATE", payload);
        var imageData = new byte[] { 0x01, 0x02, 0x03 };

        SetupFanfouUserAccount(1, 1);
        _mockTextToImageService
            .Setup(s => s.GenerateImageAsync(longContent, false, 600, It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageData);
        _mockFanfouStatusService
            .Setup(s => s.SendStatusWithPhotoAsync(TestConsumerKey, TestConsumerSecret, TestAccessToken, TestAccessTokenSecret, imageData, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("status456");

        var testNote = new Note { Id = 123, FanfouStatusIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _fanfouSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTextToImageService.Verify(s => s.GenerateImageAsync(longContent, false, 600, It.IsAny<CancellationToken>()), Times.Once);
        _mockFanfouStatusService.Verify(s => s.SendStatusWithPhotoAsync(TestConsumerKey, TestConsumerSecret, TestAccessToken, TestAccessTokenSecret, imageData, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DELETE Action Tests

    [Test]
    public async Task ProcessDeleteAction_ShouldDeleteStatusAndUpdateNote()
    {
        // Arrange
        var payload = new FanfouSyncPayload
        {
            UserAccountId = 1,
            StatusId = "status123"
        };

        var task = CreateSyncTask("DELETE", payload);

        SetupFanfouUserAccount(1, 1);
        _mockFanfouStatusService
            .Setup(s => s.DeleteStatusAsync(TestConsumerKey, TestConsumerSecret, TestAccessToken, TestAccessTokenSecret, payload.StatusId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var testNote = new Note { Id = 123, FanfouStatusIds = $"1:{payload.StatusId}" };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _fanfouSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockFanfouStatusService.Verify(s => s.DeleteStatusAsync(TestConsumerKey, TestConsumerSecret, TestAccessToken, TestAccessTokenSecret, payload.StatusId, It.IsAny<CancellationToken>()), Times.Once);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, Note>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
    }

    [Test]
    public async Task ProcessDeleteAction_WithoutStatusId_ShouldReturnFailure()
    {
        // Arrange
        var payload = new FanfouSyncPayload
        {
            UserAccountId = 1,
            StatusId = null // Missing StatusId
        };

        var task = CreateSyncTask("DELETE", payload);

        SetupFanfouUserAccount(1, 1);

        // Act
        var result = await _fanfouSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("StatusId is required"));
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public async Task ProcessAsync_WithInvalidPayload_ShouldReturnFailure()
    {
        // Arrange
        var invalidTask = new SyncTask
        {
            Id = Guid.NewGuid().ToString(),
            Service = "fanfou",
            Action = "CREATE",
            EntityId = 123,
            UserId = 1,
            Payload = "invalid_payload"
        };

        SetupFanfouUserAccount(1, 1);

        // Act
        var result = await _fanfouSyncHandler.ProcessAsync(invalidTask, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Invalid payload type"));
        Assert.That(result.ShouldRetry, Is.False);
    }

    [Test]
    public async Task ProcessAsync_WithNoFanfouAccount_ShouldReturnFailure()
    {
        // Arrange
        var payload = new FanfouSyncPayload
        {
            FullContent = "Test message",
            UserAccountId = 1,
            IsMarkdown = false
        };

        var task = CreateSyncTask("CREATE", payload);

        _mockFanfouUserAccountCacheService
            .Setup(s => s.GetAsync(1))
            .ReturnsAsync(new List<FanfouUserAccount>());

        // Act
        var result = await _fanfouSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("No Fanfou user account found"));
        Assert.That(result.ShouldRetry, Is.False);
    }

    [Test]
    public async Task ProcessAsync_WithUnknownAction_ShouldReturnFailure()
    {
        // Arrange
        var payload = new FanfouSyncPayload
        {
            FullContent = "Test message",
            UserAccountId = 1,
            IsMarkdown = false
        };

        var task = CreateSyncTask("UNKNOWN_ACTION", payload);

        SetupFanfouUserAccount(1, 1);

        // Act
        var result = await _fanfouSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Unknown action: UNKNOWN_ACTION"));
    }

    #endregion

    #region Retry Delay Tests

    [Test]
    public void CalculateRetryDelay_ShouldUseExponentialBackoff()
    {
        // Act & Assert
        var delay1 = _fanfouSyncHandler.CalculateRetryDelay(1);
        var delay2 = _fanfouSyncHandler.CalculateRetryDelay(2);
        var delay3 = _fanfouSyncHandler.CalculateRetryDelay(3);

        // Exponential backoff: each delay should be longer than the previous
        Assert.That(delay2.TotalSeconds, Is.GreaterThan(delay1.TotalSeconds));
        Assert.That(delay3.TotalSeconds, Is.GreaterThan(delay2.TotalSeconds));
    }

    #endregion
}

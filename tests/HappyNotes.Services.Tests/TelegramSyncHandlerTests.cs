using System.Text.Json;
using System.Threading;
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
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace HappyNotes.Services.Tests;

public class TelegramSyncHandlerTests
{
    private Mock<ITelegramService> _mockTelegramService;
    private Mock<ITelegramSettingsCacheService> _mockTelegramSettingsCache;
    private Mock<INoteRepository> _mockNoteRepository;
    private Mock<IOptions<SyncQueueOptions>> _mockSyncQueueOptions;
    private Mock<IOptions<JwtConfig>> _mockJwtConfig;
    private Mock<ILogger<TelegramSyncHandler>> _mockLogger;
    private TelegramSyncHandler _telegramSyncHandler;

    private const string TestBotToken = "test_bot_token";
    private const string TestChannelId = "-1001234567890";
    private const string TestJwtKey = "test_key_1234567890123456";

    [SetUp]
    public void Setup()
    {
        _mockTelegramService = new Mock<ITelegramService>();
        _mockTelegramSettingsCache = new Mock<ITelegramSettingsCacheService>();
        _mockNoteRepository = new Mock<INoteRepository>();
        _mockSyncQueueOptions = new Mock<IOptions<SyncQueueOptions>>();
        _mockJwtConfig = new Mock<IOptions<JwtConfig>>();
        _mockLogger = new Mock<ILogger<TelegramSyncHandler>>();

        var syncQueueOptions = new SyncQueueOptions();
        _mockSyncQueueOptions.Setup(x => x.Value).Returns(syncQueueOptions);

        var jwtConfig = new JwtConfig { SymmetricSecurityKey = TestJwtKey };
        _mockJwtConfig.Setup(x => x.Value).Returns(jwtConfig);

        _telegramSyncHandler = new TelegramSyncHandler(
            _mockTelegramService.Object,
            _mockTelegramSettingsCache.Object,
            _mockNoteRepository.Object,
            _mockSyncQueueOptions.Object,
            _mockJwtConfig.Object,
            _mockLogger.Object
        );
    }

    private SyncTask CreateSyncTask(string action, TelegramSyncPayload payload, long noteId = 123, long userId = 1)
    {
        var task = SyncTask.Create("telegram", action, noteId, userId, payload);
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

    private void SetupTelegramSettings(long userId, string channelId, string botToken)
    {
        var telegramSettings = new List<TelegramSettings>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                ChannelId = channelId,
                EncryptedToken = TextEncryptionHelper.Encrypt(botToken, TestJwtKey)
            }
        };

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(userId))
            .ReturnsAsync(telegramSettings);
    }

    #region CREATE Action Tests

    [Test]
    public async Task ProcessCreateAction_WithShortContent_ShouldSendMessage()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "CREATE",
            FullContent = "Short test message",
            ChannelId = TestChannelId,
            IsMarkdown = false
        };

        var task = CreateSyncTask("CREATE", payload);
        var testMessage = new Message { MessageId = 100 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, payload.IsMarkdown, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testMessage);

        var testNote = new Note { Id = 123, TelegramMessageIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, payload.IsMarkdown, It.IsAny<CancellationToken>()), Times.Once);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, Note>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
    }

    [Test]
    public async Task ProcessCreateAction_WithLongContent_ShouldSendAsFile()
    {
        // Arrange
        var longContent = new string('a', 5000); // > 4096 chars
        var payload = new TelegramSyncPayload
        {
            Action = "CREATE",
            FullContent = longContent,
            ChannelId = TestChannelId,
            IsMarkdown = true
        };

        var task = CreateSyncTask("CREATE", payload);
        var testMessage = new Message { MessageId = 200 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".md", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testMessage);

        var testNote = new Note { Id = 123, TelegramMessageIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".md", It.IsAny<CancellationToken>()), Times.Once);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, Note>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
    }

    [Test]
    public async Task ProcessCreateAction_ShortContent_MarkdownFallback_ShouldRetryWithPlainText()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "CREATE",
            FullContent = "Test message with **markdown**",
            ChannelId = TestChannelId,
            IsMarkdown = true
        };

        var task = CreateSyncTask("CREATE", payload);
        var testMessage = new Message { MessageId = 300 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);

        // First call with Markdown fails
        _mockTelegramService
            .Setup(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException("Bad Request: can't parse entities", 400));

        // Second call with plain text succeeds
        _mockTelegramService
            .Setup(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testMessage);

        var testNote = new Note { Id = 123, TelegramMessageIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, true, It.IsAny<CancellationToken>()), Times.Once);
        _mockTelegramService.Verify(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessCreateAction_LongContent_MarkdownFallback_ShouldRetryWithTxtFile()
    {
        // Arrange
        var longContent = new string('a', 5000); // > 4096 chars
        var payload = new TelegramSyncPayload
        {
            Action = "CREATE",
            FullContent = longContent,
            ChannelId = TestChannelId,
            IsMarkdown = true
        };

        var task = CreateSyncTask("CREATE", payload);
        var testMessage = new Message { MessageId = 400 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);

        // First call with .md file fails
        _mockTelegramService
            .Setup(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".md", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException("Bad Request: can't parse entities", 400));

        // Second call with .txt file succeeds
        _mockTelegramService
            .Setup(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testMessage);

        var testNote = new Note { Id = 123, TelegramMessageIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".md", It.IsAny<CancellationToken>()), Times.Once);
        _mockTelegramService.Verify(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".txt", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UPDATE Action Tests

    [Test]
    public async Task ProcessUpdateAction_WithShortContent_ShouldEditMessage()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "UPDATE",
            FullContent = "Updated short message",
            ChannelId = TestChannelId,
            MessageId = 100,
            IsMarkdown = false
        };

        var task = CreateSyncTask("UPDATE", payload);

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.EditMessageAsync(TestBotToken, TestChannelId, 100, payload.FullContent, payload.IsMarkdown, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { MessageId = 100 });

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.EditMessageAsync(TestBotToken, TestChannelId, 100, payload.FullContent, payload.IsMarkdown, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessUpdateAction_WithLongContent_ShouldDeleteAndRecreate()
    {
        // Arrange
        var longContent = new string('a', 5000); // > 4096 chars
        var payload = new TelegramSyncPayload
        {
            Action = "UPDATE",
            FullContent = longContent,
            ChannelId = TestChannelId,
            MessageId = 100,
            IsMarkdown = false
        };

        var task = CreateSyncTask("UPDATE", payload);
        var newMessage = new Message { MessageId = 500 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockTelegramService
            .Setup(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(newMessage);

        var testNote = new Note { Id = 123, TelegramMessageIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()), Times.Once);
        _mockTelegramService.Verify(s => s.SendLongMessageAsFileAsync(TestBotToken, TestChannelId, longContent, ".txt", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessUpdateAction_ShortContent_MarkdownFallback_ShouldRetryWithPlainText()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "UPDATE",
            FullContent = "Updated **markdown** message",
            ChannelId = TestChannelId,
            MessageId = 100,
            IsMarkdown = true
        };

        var task = CreateSyncTask("UPDATE", payload);

        SetupTelegramSettings(1, TestChannelId, TestBotToken);

        // First call with Markdown fails
        _mockTelegramService
            .Setup(s => s.EditMessageAsync(TestBotToken, TestChannelId, 100, payload.FullContent, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException("Bad Request: can't parse entities", 400));

        // Second call with plain text succeeds
        _mockTelegramService
            .Setup(s => s.EditMessageAsync(TestBotToken, TestChannelId, 100, payload.FullContent, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { MessageId = 100 });

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.EditMessageAsync(TestBotToken, TestChannelId, 100, payload.FullContent, true, It.IsAny<CancellationToken>()), Times.Once);
        _mockTelegramService.Verify(s => s.EditMessageAsync(TestBotToken, TestChannelId, 100, payload.FullContent, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessUpdateAction_WithoutMessageId_ShouldFallbackToCreate()
    {
        // Arrange - This tests race condition handling: UPDATE triggered before CREATE completed
        var payload = new TelegramSyncPayload
        {
            Action = "UPDATE",
            FullContent = "Test content",
            ChannelId = TestChannelId,
            MessageId = null, // Missing MessageId (race condition)
            IsMarkdown = false
        };

        var task = CreateSyncTask("UPDATE", payload);
        var testMessage = new Message { MessageId = 100 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);

        // Note has no existing message (simulating race condition)
        var testNote = new Note { Id = 123, TelegramMessageIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        _mockTelegramService
            .Setup(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testMessage);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert - Should succeed by falling back to CREATE
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DELETE Action Tests

    [Test]
    public async Task ProcessDeleteAction_ShouldDeleteMessageAndUpdateNote()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "DELETE",
            ChannelId = TestChannelId,
            MessageId = 100
        };

        var task = CreateSyncTask("DELETE", payload);

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var testNote = new Note { Id = 123, TelegramMessageIds = $"{TestChannelId}:100" };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()), Times.Once);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, Note>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
    }

    [Test]
    public async Task ProcessDeleteAction_WithoutMessageId_ShouldThrowException()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "DELETE",
            ChannelId = TestChannelId,
            MessageId = null // Missing MessageId
        };

        var task = CreateSyncTask("DELETE", payload);

        SetupTelegramSettings(1, TestChannelId, TestBotToken);

        // Act & Assert
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("MessageId is required for DELETE action"));
    }

    [Test]
    public async Task ProcessDeleteAction_MessageCantBeDeleted_ShouldTreatAsSuccess()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "DELETE",
            ChannelId = TestChannelId,
            MessageId = 100
        };

        var task = CreateSyncTask("DELETE", payload);
        var testNote = new Note { Id = task.EntityId, TelegramMessageIds = $"{TestChannelId}:100" };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockNoteRepository.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Setup DeleteMessageAsync to throw "message can't be deleted" exception
        _mockTelegramService.Setup(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException("Bad Request: message can't be deleted", 400));

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()), Times.Once);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, Note>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()), Times.Once);
    }

    [Test]
    public async Task ProcessDeleteAction_MessageToDeleteNotFound_ShouldTreatAsSuccess()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "DELETE",
            ChannelId = TestChannelId,
            MessageId = 100
        };

        var task = CreateSyncTask("DELETE", payload);
        var testNote = new Note { Id = task.EntityId, TelegramMessageIds = $"{TestChannelId}:100" };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockNoteRepository.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Setup DeleteMessageAsync to throw "message to delete not found" exception
        _mockTelegramService.Setup(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException("Bad Request: message to delete not found", 400));

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockTelegramService.Verify(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()), Times.Once);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, Note>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()), Times.Once);
    }

    #endregion

    #region Database Update Tests

    [Test]
    public async Task AddMessageIdToNote_WithEmptyTelegramMessageIds_ShouldSetNewValue()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "CREATE",
            FullContent = "Test message",
            ChannelId = TestChannelId,
            IsMarkdown = false
        };

        var task = CreateSyncTask("CREATE", payload);
        var testMessage = new Message { MessageId = 100 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testMessage);

        var testNote = new Note { Id = 123, TelegramMessageIds = null };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.Is<System.Linq.Expressions.Expression<Func<Note, Note>>>(expr =>
                expr.Compile()(new Note()).TelegramMessageIds == $"{TestChannelId}:100"),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
    }

    [Test]
    public async Task AddMessageIdToNote_WithExistingTelegramMessageIds_ShouldAppendNewValue()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "CREATE",
            FullContent = "Test message",
            ChannelId = TestChannelId,
            IsMarkdown = false
        };

        var task = CreateSyncTask("CREATE", payload);
        var testMessage = new Message { MessageId = 200 };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.SendMessageAsync(TestBotToken, TestChannelId, payload.FullContent, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testMessage);

        var testNote = new Note { Id = 123, TelegramMessageIds = "-1009876543210:150" };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.Is<System.Linq.Expressions.Expression<Func<Note, Note>>>(expr =>
                expr.Compile()(new Note()).TelegramMessageIds!.Contains($"{TestChannelId}:200")),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
    }

    [Test]
    public async Task RemoveMessageIdFromNote_ShouldRemoveSpecificMessageId()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "DELETE",
            ChannelId = TestChannelId,
            MessageId = 100
        };

        var task = CreateSyncTask("DELETE", payload);

        SetupTelegramSettings(1, TestChannelId, TestBotToken);
        _mockTelegramService
            .Setup(s => s.DeleteMessageAsync(TestBotToken, TestChannelId, 100, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var testNote = new Note { Id = 123, TelegramMessageIds = $"-1009876543210:150,{TestChannelId}:100" };
        _mockNoteRepository
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>(), null))
            .ReturnsAsync(testNote);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockNoteRepository.Verify(r => r.UpdateAsync(
            It.Is<System.Linq.Expressions.Expression<Func<Note, Note>>>(expr =>
                expr.Compile()(new Note()).TelegramMessageIds == "-1009876543210:150"),
            It.IsAny<System.Linq.Expressions.Expression<Func<Note, bool>>>()
        ), Times.Once);
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
            Service = "telegram",
            Action = "CREATE",
            EntityId = 123,
            UserId = 1,
            Payload = "invalid_payload"
        };

        SetupTelegramSettings(1, TestChannelId, TestBotToken);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(invalidTask, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("Invalid payload type"));
        Assert.That(result.ShouldRetry, Is.False);
    }

    [Test]
    public async Task ProcessAsync_WithNoTelegramSettings_ShouldReturnFailure()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "CREATE",
            FullContent = "Test message",
            ChannelId = TestChannelId,
            IsMarkdown = false
        };

        var task = CreateSyncTask("CREATE", payload);

        _mockTelegramSettingsCache
            .Setup(s => s.GetAsync(1))
            .ReturnsAsync(new List<TelegramSettings>());

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("No Telegram settings found"));
        Assert.That(result.ShouldRetry, Is.False);
    }

    [Test]
    public async Task ProcessAsync_WithUnknownAction_ShouldReturnFailure()
    {
        // Arrange
        var payload = new TelegramSyncPayload
        {
            Action = "UNKNOWN_ACTION",
            FullContent = "Test message",
            ChannelId = TestChannelId,
            IsMarkdown = false
        };

        var task = CreateSyncTask("UNKNOWN_ACTION", payload);

        SetupTelegramSettings(1, TestChannelId, TestBotToken);

        // Act
        var result = await _telegramSyncHandler.ProcessAsync(task, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("Unknown action: UNKNOWN_ACTION"));
    }

    #endregion
}

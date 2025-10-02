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

public class MastodonSyncNoteServiceTests
{
    private Mock<IMastodonTootService> _mockMastodonTootService;
    private Mock<INoteRepository> _mockNoteRepository;
    private Mock<IMastodonUserAccountCacheService> _mockMastodonUserAccountCacheService;
    private Mock<IOptions<JwtConfig>> _mockJwtConfig;
    private Mock<ISyncQueueService> _mockSyncQueueService;
    private Mock<ILogger<MastodonSyncNoteService>> _mockLogger;
    private MastodonSyncNoteService _mastodonSyncNoteService;

    [SetUp]
    public void Setup()
    {
        _mockMastodonTootService = new Mock<IMastodonTootService>();
        _mockNoteRepository = new Mock<INoteRepository>();
        _mockMastodonUserAccountCacheService = new Mock<IMastodonUserAccountCacheService>();
        _mockJwtConfig = new Mock<IOptions<JwtConfig>>();
        _mockSyncQueueService = new Mock<ISyncQueueService>();
        _mockLogger = new Mock<ILogger<MastodonSyncNoteService>>();

        var jwtConfig = new JwtConfig { SymmetricSecurityKey = "test_key" };
        _mockJwtConfig.Setup(x => x.Value).Returns(jwtConfig);

        _mastodonSyncNoteService = new MastodonSyncNoteService(
            _mockMastodonTootService.Object,
            _mockNoteRepository.Object,
            _mockMastodonUserAccountCacheService.Object,
            _mockJwtConfig.Object,
            _mockSyncQueueService.Object,
            _mockLogger.Object
        );
    }

    [TestCase("test note", true, MastodonSyncType.All, "", true)]
    [TestCase("test note", false, MastodonSyncType.All, "", true)]
    [TestCase("test note", false, MastodonSyncType.PublicOnly, "", true)]
    [TestCase("test note", true, MastodonSyncType.PublicOnly, "", false)]
    [TestCase("test note", true, MastodonSyncType.TagMastodonOnly, "mastodon", true)]
    [TestCase("test note", false, MastodonSyncType.TagMastodonOnly, "mastodon", true)]
    [TestCase("test note", false, MastodonSyncType.TagMastodonOnly, "", false)]
    [TestCase("test note", true, MastodonSyncType.TagMastodonOnly, "", false)]
    public async Task SyncNewNote_ShouldSyncNoteToMastodon(string fullContent, bool isPrivate,
        MastodonSyncType syncType, string tag, bool shouldSync)
    {
        // Arrange
        var note = new Note
        {
            UserId = 1,
            IsPrivate = isPrivate,
            TagList = [tag,]
        };

        var mastodonUserAccounts = new List<MastodonUserAccount>
        {
            new()
            {
                Id = 1, UserId = 1, AccessToken = TextEncryptionHelper.Encrypt("test token", "test_key"),
                InstanceUrl = "https://mastodon.instance", SyncType = syncType
            }
        };

        _mockMastodonUserAccountCacheService
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(mastodonUserAccounts);

        // Act
        await _mastodonSyncNoteService.SyncNewNote(note, fullContent);

        // Assert - Verify queue operation instead of direct database update
        if (shouldSync)
        {
            _mockSyncQueueService.Verify(s => s.EnqueueAsync("mastodon",
                It.Is<SyncTask<MastodonSyncPayload>>(task =>
                    task.Action == "CREATE" &&
                    task.EntityId == note.Id &&
                    task.UserId == note.UserId)),
                Times.Once);
        }
        else
        {
            _mockSyncQueueService.Verify(s => s.EnqueueAsync(It.IsAny<string>(), It.IsAny<SyncTask<object>>()), Times.Never);
        }
    }

    [TestCase("@123 This is content", "This is content")]
    [TestCase("@123 @456 This is content", "This is content")]
    [TestCase("This is content @123", "This is content")]
    [TestCase("@123 This is content @456", "This is content")]
    [TestCase("This @123 is content", "This @123 is content")]
    public async Task SyncNewNote_WithNoteLinks_ShouldEnqueueWithOriginalContent(string fullContent, string expectedCleanContent)
    {
        // Arrange
        var note = new Note
        {
            Id = 100,
            UserId = 1,
            IsPrivate = false,
            TagList = []
        };

        var mastodonUserAccounts = new List<MastodonUserAccount>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                AccessToken = TextEncryptionHelper.Encrypt("test token", "test_key"),
                InstanceUrl = "https://mastodon.instance",
                SyncType = MastodonSyncType.All
            }
        };

        _mockMastodonUserAccountCacheService
            .Setup(s => s.GetAsync(note.UserId))
            .ReturnsAsync(mastodonUserAccounts);

        // Act
        await _mastodonSyncNoteService.SyncNewNote(note, fullContent);

        // Assert - The original content should be enqueued (cleaning happens in MastodonTootService)
        _mockSyncQueueService.Verify(s => s.EnqueueAsync("mastodon",
            It.Is<SyncTask<MastodonSyncPayload>>(task =>
                task.Action == "CREATE" &&
                task.EntityId == note.Id &&
                task.UserId == note.UserId &&
                ((MastodonSyncPayload)task.Payload).FullContent == fullContent)),
            Times.Once);
    }
}

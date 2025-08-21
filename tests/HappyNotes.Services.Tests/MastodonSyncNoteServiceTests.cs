using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using Mastonet.Entities;
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
    private Mock<ILogger<MastodonSyncNoteService>> _mockLogger;
    private MastodonSyncNoteService _mastodonSyncNoteService;

    [SetUp]
    public void Setup()
    {
        _mockMastodonTootService = new Mock<IMastodonTootService>();
        _mockNoteRepository = new Mock<INoteRepository>();
        _mockMastodonUserAccountCacheService = new Mock<IMastodonUserAccountCacheService>();
        _mockJwtConfig = new Mock<IOptions<JwtConfig>>();
        _mockLogger = new Mock<ILogger<MastodonSyncNoteService>>();

        var jwtConfig = new JwtConfig { SymmetricSecurityKey = "test_key" };
        _mockJwtConfig.Setup(x => x.Value).Returns(jwtConfig);

        _mastodonSyncNoteService = new MastodonSyncNoteService(
            _mockMastodonTootService.Object,
            _mockNoteRepository.Object,
            _mockMastodonUserAccountCacheService.Object,
            _mockJwtConfig.Object,
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

        _mockMastodonTootService
            .Setup(s => s.SendTootAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), false))
            .ReturnsAsync(new Status { Id = "12345" });

        // Act
        await _mastodonSyncNoteService.SyncNewNote(note, fullContent);

        // Assert
        _mockNoteRepository.Verify(repo => repo.UpdateAsync(It.Is<Note>(n => n.MastodonTootIds!.Contains("12345"))),
            shouldSync ? Times.Once : Times.Never);
    }
}

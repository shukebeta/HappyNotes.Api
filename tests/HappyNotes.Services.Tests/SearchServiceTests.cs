using System.Net;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using Moq;
using Moq.Protected;
using SqlSugar;

namespace HappyNotes.Services.Tests;

[TestFixture]
public class SearchServiceTests
{
    private readonly Mock<IDatabaseClient> _mockDatabaseClient;
    private readonly SearchService _searchService;
    private readonly Mock<HttpMessageHandler> _mockHandler;

    public SearchServiceTests()
    {
        _mockDatabaseClient = new Mock<IDatabaseClient>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":0,\"hits\":[]}}")
            });
        var httpClient = new HttpClient(_mockHandler.Object);
        var options = new ManticoreConnectionOptions { HttpEndpoint = "http://127.0.0.1:9308" };
        _searchService = new SearchService(_mockDatabaseClient.Object, httpClient, options);
    }

    [SetUp]
    public void Setup()
    {
        // Reset mock invocations before each test to avoid cross-test interference.
        _mockDatabaseClient.Reset();
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":0,\"hits\":[]}}")
            });
    }

    [Test]
    public async Task SearchNotesAsync_NormalFilter_ReturnsPaginatedResults()
    {
        // Arrange
        long userId = 1;
        string query = "test";
        int pageNumber = 1;
        int pageSize = 10;
        NoteFilterType filter = NoteFilterType.Normal;
        var expectedNoteIds = new List<long> { 1, 2 };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":2,\"hits\":[{\"_id\":1,\"_source\":{\"id\":1,\"userid\":1,\"content\":\"Test note 1\"}},{\"_id\":2,\"_source\":{\"id\":2,\"userid\":1,\"content\":\"Test note 2\"}}]}}")
            });

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item2, Is.EqualTo(2));
        CollectionAssert.AreEqual(new List<long> { 1, 2 }, result.Item1);
    }

    [Test]
    public async Task SearchNotesAsync_DeletedFilter_ReturnsDeletedNotes()
    {
        // Arrange
        long userId = 1;
        string query = "deleted note";
        int pageNumber = 1;
        int pageSize = 10;
        NoteFilterType filter = NoteFilterType.Deleted;
        var expectedNoteIds = new List<long> { 3 };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":1,\"hits\":[{\"_id\":3,\"_source\":{\"id\":3,\"userid\":1,\"content\":\"Deleted note 1\"}}]}}")
            });

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item2, Is.EqualTo(1));
        CollectionAssert.AreEqual(new List<long> { 3 }, result.Item1);
    }

    [Test]
    public async Task SearchNotesAsync_EmptyResults_ReturnsEmptyPageData()
    {
        // Arrange
        long userId = 1;
        string query = "nonexistent";
        int pageNumber = 1;
        int pageSize = 10;
        NoteFilterType filter = NoteFilterType.Normal;
        var expectedNoteIds = new List<long>();

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":0,\"hits\":[]}}")
            });

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.IsEmpty(result.Item1);
        Assert.That(result.Item2, Is.EqualTo(0));
    }

    [Test]
    public async Task SyncNoteToIndexAsync_SuccessfulInsertion_DoesNotThrow()
    {
        // Arrange
        var note = new Note
        {
            Id = 1,
            UserId = 1,
            Content = "Test content",
            CreatedAt = 1625097600,
        };
        string fullContent = "Test content";

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.IsAny<SugarParameter[]>()))
            .ReturnsAsync(1); // Simulate successful execution

        // Act
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.Is<SugarParameter[]>(p =>
                p.Any(sp => sp.ParameterName == "@id" && (long)sp.Value == note.Id) &&
                p.Any(sp => sp.ParameterName == "@content" && (string)sp.Value == fullContent)
            )), Times.AtLeastOnce);
    }

    [Test]
    public async Task SyncNoteToIndexAsync_SpecialCharacters_HandlesEscaping()
    {
        // Arrange
        var note = new Note
        {
            Id = 2,
            UserId = 1,
            Content = @"Content with 'quotes' and \slashes\",
            CreatedAt = 1625097600,
        };
        string fullContent = @"Content with 'quotes' and \slashes\";

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.IsAny<SugarParameter[]>()))
            .ReturnsAsync(1);

        // Act
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.Is<SugarParameter[]>(p =>
                p.Any(sp => sp.ParameterName == "@id" && (long)sp.Value == note.Id) &&
                p.Any(sp => sp.ParameterName == "@content" && (string)sp.Value == fullContent)
            )), Times.AtLeastOnce);
    }

    [Test]
    public async Task DeleteNoteFromIndexAsync_ExistingNote_UpdatesDeletedAt()
    {
        // Arrange
        long noteId = 1;

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat")),
            It.Is<object>(param => param.ToString()!.Contains(noteId.ToString()))))
            .ReturnsAsync(1);

        // Act
        await _searchService.DeleteNoteFromIndexAsync(noteId);

        // Assert
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat")),
            It.Is<object>(param => param.ToString()!.Contains(noteId.ToString()))), Times.Once);
    }

    [Test]
    public async Task UndeleteNoteFromIndexAsync_DeletedNote_ResetsDeletedAt()
    {
        // Arrange
        long noteId = 1;

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat = 0")),
            It.Is<object>(param => param.ToString()!.Contains(noteId.ToString()))))
            .ReturnsAsync(1);

        // Act
        await _searchService.UndeleteNoteFromIndexAsync(noteId);

        // Assert
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat = 0")),
            It.Is<object>(param => param.ToString()!.Contains(noteId.ToString()))), Times.Once);
    }

    [Test]
    public async Task PurgeDeletedNotesFromIndexAsync_DeletedNotes_RemovesThem()
    {
        // Arrange
        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("DELETE FROM noteindex WHERE deletedAt > 0")),
            It.IsAny<object>()))
            .ReturnsAsync(10); // Simulate deleting 10 records

        // Act
        await _searchService.PurgeDeletedNotesFromIndexAsync();

        // Assert
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("DELETE FROM noteindex WHERE deletedAt > 0")),
            It.IsAny<object>()), Times.Once);
    }
}

using System.Net;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using HappyNotes.Repositories.interfaces;
using Moq;
using Moq.Protected;
using SqlSugar;

namespace HappyNotes.Services.Tests;

[TestFixture]
public class SearchServiceTests
{
    private readonly Mock<IDatabaseClient> _mockDatabaseClient;
    private readonly Mock<INoteRepository> _mockNoteRepository;
    private readonly SearchService _searchService;
    private readonly Mock<HttpMessageHandler> _mockHandler;

    public SearchServiceTests()
    {
        _mockDatabaseClient = new Mock<IDatabaseClient>();
        _mockNoteRepository = new Mock<INoteRepository>();
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
        _searchService = new SearchService(_mockDatabaseClient.Object, httpClient, options, _mockNoteRepository.Object);
    }

    [SetUp]
    public void Setup()
    {
        // Reset mock invocations before each test to avoid cross-test interference.
        _mockDatabaseClient.Reset();
        _mockNoteRepository.Reset();
        _mockHandler.Reset();
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

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("json/replace")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":\"success\"}")
            });

        // Act
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("json/replace")),
            ItExpr.IsAny<CancellationToken>());
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

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("json/replace")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":\"success\"}")
            });

        // Act
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert - Verify HTTP API call was made
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("json/replace")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task DeleteNoteFromIndexAsync_ExistingNote_UpdatesDeletedAt()
    {
        // Arrange
        long noteId = 1;

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("json/update")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":\"success\"}")
            });

        // Act
        await _searchService.DeleteNoteFromIndexAsync(noteId);

        // Assert
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("json/update")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task UndeleteNoteFromIndexAsync_DeletedNote_ResetsDeletedAt()
    {
        // Arrange
        long noteId = 1;

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("json/update")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":\"success\"}")
            });

        // Act
        await _searchService.UndeleteNoteFromIndexAsync(noteId);

        // Assert
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("json/update")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task PurgeDeletedNotesFromIndexAsync_DeletedNotes_RemovesThem()
    {
        // Arrange
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("json/delete")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"deleted\":10}")
            });

        // Act
        await _searchService.PurgeDeletedNotesFromIndexAsync();

        // Assert
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("json/delete")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task GetNoteIdsByKeywordAsync_IntegerQuery_FirstPage_UserOwnsNote_PrependsToResults()
    {
        // Arrange
        long userId = 1;
        string query = "123";
        int pageNumber = 1;
        int pageSize = 10;

        // Mock regular search results
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":2,\"hits\":[{\"_id\":456,\"_source\":{\"id\":456}},{\"_id\":789,\"_source\":{\"id\":789}}]}}")
            });

        // Mock repository query to return user's note
        _mockNoteRepository.Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Note, bool>>>(), null))
            .ReturnsAsync(new Note { Id = 123, UserId = 1 });

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item1[0], Is.EqualTo(123)); // Note 123 should be first
        Assert.That(result.Item1.Count, Is.EqualTo(3)); // Should have 3 notes total
        CollectionAssert.AreEqual(new List<long> { 123, 456, 789 }, result.Item1);
    }

    [Test]
    public async Task GetNoteIdsByKeywordAsync_IntegerQuery_FirstPage_UserDoesNotOwnNote_DoesNotPrepend()
    {
        // Arrange
        long userId = 1;
        string query = "123";
        int pageNumber = 1;
        int pageSize = 10;

        // Mock regular search results
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":2,\"hits\":[{\"_id\":456,\"_source\":{\"id\":456}},{\"_id\":789,\"_source\":{\"id\":789}}]}}")
            });

        // Mock repository query to return note owned by different user
        _mockNoteRepository.Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Note, bool>>>(), null))
            .ReturnsAsync(new Note { Id = 123, UserId = 2 }); // Different user

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item1.Count, Is.EqualTo(2)); // Should remain 2 notes
        CollectionAssert.AreEqual(new List<long> { 456, 789 }, result.Item1);
    }

    [Test]
    public async Task GetNoteIdsByKeywordAsync_IntegerQuery_SecondPage_DoesNotPrepend()
    {
        // Arrange
        long userId = 1;
        string query = "123";
        int pageNumber = 2; // Not first page
        int pageSize = 10;

        // Mock regular search results
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":2,\"hits\":[{\"_id\":456,\"_source\":{\"id\":456}},{\"_id\":789,\"_source\":{\"id\":789}}]}}")
            });

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item1.Count, Is.EqualTo(2)); // Should remain 2 notes
        CollectionAssert.AreEqual(new List<long> { 456, 789 }, result.Item1);
        // Verify database was not called since it's not first page
        _mockNoteRepository.Verify(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Note, bool>>>(), null), Times.Never);
    }

    [Test]
    public async Task GetNoteIdsByKeywordAsync_IntegerQuery_NoteNotFound_DoesNotPrepend()
    {
        // Arrange
        long userId = 1;
        string query = "123";
        int pageNumber = 1;
        int pageSize = 10;

        // Mock regular search results
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":2,\"hits\":[{\"_id\":456,\"_source\":{\"id\":456}},{\"_id\":789,\"_source\":{\"id\":789}}]}}")
            });

        // Mock repository query to return null (note not found)
        _mockNoteRepository.Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Note, bool>>>(), null))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item1.Count, Is.EqualTo(2)); // Should remain 2 notes
        CollectionAssert.AreEqual(new List<long> { 456, 789 }, result.Item1);
    }

    [Test]
    public async Task GetNoteIdsByKeywordAsync_IntegerQuery_FirstPage_NoteAlreadyInResults_DoesNotDuplicate()
    {
        // Arrange
        long userId = 1;
        string query = "123";
        int pageNumber = 1;
        int pageSize = 10;

        // Mock regular search results including the note ID we're searching for
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":3,\"hits\":[{\"_id\":456,\"_source\":{\"id\":456}},{\"_id\":123,\"_source\":{\"id\":123}},{\"_id\":789,\"_source\":{\"id\":789}}]}}")
            });

        // Mock repository query to return user's note
        _mockNoteRepository.Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Note, bool>>>(), null))
            .ReturnsAsync(new Note { Id = 123, UserId = 1 });

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item1[0], Is.EqualTo(123)); // Note 123 should be first
        Assert.That(result.Item1.Count, Is.EqualTo(3)); // Should have 3 notes total (no duplication)
        CollectionAssert.AreEqual(new List<long> { 123, 456, 789 }, result.Item1);
    }

    [Test]
    public async Task GetNoteIdsByKeywordAsync_NonIntegerQuery_DoesNotPrepend()
    {
        // Arrange
        long userId = 1;
        string query = "test search";
        int pageNumber = 1;
        int pageSize = 10;

        // Mock regular search results
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"took\":0,\"timed_out\":false,\"hits\":{\"total\":2,\"hits\":[{\"_id\":456,\"_source\":{\"id\":456}},{\"_id\":789,\"_source\":{\"id\":789}}]}}")
            });

        // Act
        var result = await _searchService.GetNoteIdsByKeywordAsync(userId, query, pageNumber, pageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.Item1.Count, Is.EqualTo(2)); // Should remain 2 notes
        CollectionAssert.AreEqual(new List<long> { 456, 789 }, result.Item1);
        // Verify database was not called since query is not an integer
        _mockNoteRepository.Verify(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Note, bool>>>(), null), Times.Never);
    }
}

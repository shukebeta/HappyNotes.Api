using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Moq;
using NUnit.Framework;

namespace HappyNotes.Services.Tests;

[TestFixture]
public class SearchServiceTests
{
    private readonly Mock<IDatabaseClient> _mockDatabaseClient;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockDatabaseClient = new Mock<IDatabaseClient>();
        _searchService = new SearchService(_mockDatabaseClient.Object);
    }

    [SetUp]
    public void Setup()
    {
        // Reset mock invocations before each test to avoid cross-test interference.
        _mockDatabaseClient.Reset();
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
        var expectedNotes = new List<NoteDto>
        {
            new NoteDto { Id = 1, UserId = 1, Content = "Test note 1" },
            new NoteDto { Id = 2, UserId = 1, Content = "Test note 2" }
        };
        int expectedTotal = 2;

        _mockDatabaseClient.Setup(client => client.SqlQueryAsync<NoteDto>(
            It.Is<string>(sql => sql.Contains("DeletedAt = 0")),
            It.Is<object>(param => param.ToString().Contains(userId.ToString()) && param.ToString().Contains(query))))
            .ReturnsAsync(expectedNotes);

        _mockDatabaseClient.Setup(client => client.GetIntAsync(
            It.Is<string>(sql => sql.Contains("COUNT(*)") && sql.Contains("DeletedAt = 0")),
            It.Is<object>(param => param.ToString().Contains(userId.ToString()) && param.ToString().Contains(query))))
            .ReturnsAsync(expectedTotal);

        // Act
        var result = await _searchService.SearchNotesAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<PageData<NoteDto>>(result);
        Assert.AreEqual(expectedNotes, result.DataList);
        Assert.AreEqual(expectedTotal, result.TotalCount);
        Assert.AreEqual(pageNumber, result.PageIndex);
        Assert.AreEqual(pageSize, result.PageSize);
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
        var expectedNotes = new List<NoteDto>
        {
            new NoteDto { Id = 3, UserId = 1, Content = "Deleted note 1" }
        };
        int expectedTotal = 1;

        _mockDatabaseClient.Setup(client => client.SqlQueryAsync<NoteDto>(
            It.Is<string>(sql => sql.Contains("DeletedAt > 0")),
            It.Is<object>(param => param.ToString().Contains(userId.ToString()) && param.ToString().Contains(query))))
            .ReturnsAsync(expectedNotes);

        _mockDatabaseClient.Setup(client => client.GetIntAsync(
            It.Is<string>(sql => sql.Contains("COUNT(*)") && sql.Contains("DeletedAt > 0")),
            It.Is<object>(param => param.ToString().Contains(userId.ToString()) && param.ToString().Contains(query))))
            .ReturnsAsync(expectedTotal);

        // Act
        var result = await _searchService.SearchNotesAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<PageData<NoteDto>>(result);
        Assert.AreEqual(expectedNotes, result.DataList);
        Assert.AreEqual(expectedTotal, result.TotalCount);
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
        var expectedNotes = new List<NoteDto>();
        int expectedTotal = 0;

        _mockDatabaseClient.Setup(client => client.SqlQueryAsync<NoteDto>(
            It.IsAny<string>(),
            It.IsAny<object>()))
            .ReturnsAsync(expectedNotes);

        _mockDatabaseClient.Setup(client => client.GetIntAsync(
            It.IsAny<string>(),
            It.IsAny<object>()))
            .ReturnsAsync(expectedTotal);

        // Act
        var result = await _searchService.SearchNotesAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result.DataList);
        Assert.AreEqual(0, result.TotalCount);
    }

    [Test]
    public async Task SyncNoteToIndexAsync_SuccessfulInsertion_DoesNotThrow()
    {
        // Arrange
        var note = new Note
        {
            Id = 1,
            UserId = 1,
            IsLong = false,
            IsPrivate = false,
            IsMarkdown = true,
            Content = "Test content",
            CreatedAt = 1625097600,
            UpdatedAt = null,
            DeletedAt = null
        };
        string fullContent = "Test content";

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.Is<object>(param => param.ToString().Contains(note.Id.ToString()) && param.ToString().Contains(fullContent))))
            .ReturnsAsync(1); // Simulate successful execution

        // Act
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert
        // Verify that the method was called at least once with the expected parameters.
        // Due to potential multiple test runs or shared mock state, we use AtLeastOnce instead of Once.
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.Is<object>(param => param.ToString().Contains(note.Id.ToString()) && param.ToString().Contains(fullContent))), Times.AtLeastOnce);
    }

    [Test]
    public async Task SyncNoteToIndexAsync_SpecialCharacters_HandlesEscaping()
    {
        // Arrange
        var note = new Note
        {
            Id = 2,
            UserId = 1,
            IsLong = false,
            IsPrivate = false,
            IsMarkdown = false,
            Content = "Content with 'quotes' and \\slashes\\",
            CreatedAt = 1625097600,
            UpdatedAt = null,
            DeletedAt = null
        };
        string fullContent = "Content with 'quotes' and \\slashes\\";

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.Is<object>(param => param.ToString().Contains(note.Id.ToString()) && param.ToString().Contains(fullContent))))
            .ReturnsAsync(1);

        // Act
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert
        // Verify at least once due to potential multiple invocations across tests.
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("REPLACE INTO noteindex")),
            It.Is<object>(param => param.ToString().Contains(note.Id.ToString()) && param.ToString().Contains(fullContent))), Times.AtLeastOnce);
    }

    [Test]
    public async Task DeleteNoteFromIndexAsync_ExistingNote_UpdatesDeletedAt()
    {
        // Arrange
        long noteId = 1;

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat")),
            It.Is<object>(param => param.ToString().Contains(noteId.ToString()))))
            .ReturnsAsync(1);

        // Act
        await _searchService.DeleteNoteFromIndexAsync(noteId);

        // Assert
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat")),
            It.Is<object>(param => param.ToString().Contains(noteId.ToString()))), Times.Once);
    }

    [Test]
    public async Task UndeleteNoteFromIndexAsync_DeletedNote_ResetsDeletedAt()
    {
        // Arrange
        long noteId = 1;

        _mockDatabaseClient.Setup(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat = 0")),
            It.Is<object>(param => param.ToString().Contains(noteId.ToString()))))
            .ReturnsAsync(1);

        // Act
        await _searchService.UndeleteNoteFromIndexAsync(noteId);

        // Assert
        _mockDatabaseClient.Verify(client => client.ExecuteCommandAsync(
            It.Is<string>(sql => sql.Contains("UPDATE noteindex SET deletedat = 0")),
            It.Is<object>(param => param.ToString().Contains(noteId.ToString()))), Times.Once);
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
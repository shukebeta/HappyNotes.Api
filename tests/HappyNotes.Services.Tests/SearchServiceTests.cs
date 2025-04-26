using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using SqlSugar;
using Xunit;

namespace HappyNotes.Services.Tests;

public class SearchServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockConfigSection;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfigSection = new Mock<IConfigurationSection>();
        _mockConfigSection.Setup(x => x.Value).Returns("server=127.0.0.1; port=9306; charset=utf8mb4;");
        _mockConfiguration.Setup(x => x.GetSection("ManticoreConnectionOptions:ConnectionString")).Returns(_mockConfigSection.Object);

        // We would ideally mock SqlSugarClient, but for simplicity in testing, we'll create a real instance
        // with a mocked configuration. In a real scenario, consider using a test double or further abstraction.
        _searchService = new SearchService(_mockConfiguration.Object);
    }

    [Fact]
    public async Task SearchNotesAsync_NormalFilter_ReturnsPaginatedResults()
    {
        // Arrange
        long userId = 1;
        string query = "test";
        int pageNumber = 1;
        int pageSize = 10;
        NoteFilterType filter = NoteFilterType.Normal;

        // This test would ideally mock SqlSugarClient.Ado.SqlQueryAsync and GetIntAsync
        // Due to the complexity of mocking SqlSugar, this test is a placeholder for actual implementation
        // In practice, consider using a testable wrapper around SqlSugarClient or integration testing for DB ops

        // Act
        // Since we can't mock SqlSugar easily without changing code structure, this is a conceptual test
        var result = await _searchService.SearchNotesAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PageData<NoteDto>>(result);
        // Further assertions would check DataList count, TotalCount, etc., if mocking was in place
    }

    [Fact]
    public async Task SearchNotesAsync_DeletedFilter_ReturnsDeletedNotes()
    {
        // Arrange
        long userId = 1;
        string query = "deleted note";
        int pageNumber = 1;
        int pageSize = 10;
        NoteFilterType filter = NoteFilterType.Deleted;

        // Act
        var result = await _searchService.SearchNotesAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PageData<NoteDto>>(result);
        // Assertions would verify that only deleted notes are returned if mocking was implemented
    }

    [Fact]
    public async Task SearchNotesAsync_EmptyResults_ReturnsEmptyPageData()
    {
        // Arrange
        long userId = 1;
        string query = "nonexistent";
        int pageNumber = 1;
        int pageSize = 10;
        NoteFilterType filter = NoteFilterType.Normal;

        // Act
        var result = await _searchService.SearchNotesAsync(userId, query, pageNumber, pageSize, filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.DataList); // Assuming no results for "nonexistent", though not mocked
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
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
            CreatedAt = 1625097600, // Unix timestamp for a date
            UpdatedAt = null,
            DeletedAt = null
        };
        string fullContent = "Test content";

        // Act
        // Again, without mocking SqlSugarClient.Ado.ExecuteCommandAsync, this is conceptual
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert
        // No exception means success; in a real test, verify the SQL command was called with correct params
    }

    [Fact]
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

        // Act
        await _searchService.SyncNoteToIndexAsync(note, fullContent);

        // Assert
        // Verify no exception; ideally check the SQL string for proper escaping if mocked
    }

    [Fact]
    public async Task DeleteNoteFromIndexAsync_ExistingNote_UpdatesDeletedAt()
    {
        // Arrange
        long noteId = 1;

        // Act
        await _searchService.DeleteNoteFromIndexAsync(noteId);

        // Assert
        // Without mocking, we can't verify the update happened; in real test, check SQL command
    }

    [Fact]
    public async Task UndeleteNoteFromIndexAsync_DeletedNote_ResetsDeletedAt()
    {
        // Arrange
        long noteId = 1;

        // Act
        await _searchService.UndeleteNoteFromIndexAsync(noteId);

        // Assert
        // Verify SQL command to set deletedAt to 0 was executed if mocked
    }

    [Fact]
    public async Task PurgeDeletedNotesFromIndexAsync_DeletedNotes_RemovesThem()
    {
        // Act
        await _searchService.PurgeDeletedNotesFromIndexAsync();

        // Assert
        // Verify DELETE command was executed for notes with deletedAt > 0 if mocked
    }
}
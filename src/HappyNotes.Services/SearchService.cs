using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyNotes.Dto;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace HappyNotes.Services;

public class SearchService : ISearchService
{
    private readonly SqlSugarClient _client;

    public SearchService(IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("ManticoreConnectionOptions:ConnectionString").Value;
        _client = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.MySql,
            IsAutoCloseConnection = true
        });
    }

    public async Task<List<NoteDto>> SearchNotesAsync(long userId, string query, int pageNumber, int pageSize)
    {
        RefAsync<int> total = 0;
        // Manticoresearch query to search in content
        var sql = "SELECT * FROM idx_notes WHERE UserId = 1 AND MATCH('Happy')";
        var list = await _client.Ado.SqlQueryAsync<NoteDto>(sql);
        return list;
    }

    public async Task SyncNoteToIndexAsync(long id, long userId, bool isPrivate, string content, long createdAt, long? updatedAt)
    {
        await _client.Ado.ExecuteCommandAsync(
            "REPLACE INTO idx_notes (Id, UserId, IsPrivate, Content, CreatedAt, UpdatedAt) VALUES (@id, @userId, @isPrivate, @content, @createdAt, @updatedAt)",
            new { id, userId, isPrivate, content, createdAt, updatedAt });
    }

    public async Task DeleteNoteFromIndexAsync(long id)
    {
        await _client.Ado.ExecuteCommandAsync("DELETE FROM idx_notes WHERE Id = @id", new { id });
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyNotes.Dto;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using Api.Framework.Models;

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
            IsAutoCloseConnection = false
        });
    }

    public async Task<PageData<NoteDto>> SearchNotesAsync(long userId, string query, int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;
        var sql = "SELECT * FROM idx_notes WHERE UserId = @userId AND MATCH(@query) LIMIT @offset, @pageSize";
        var list = await _client.Ado.SqlQueryAsync<NoteDto>(sql, new { userId, query, offset, pageSize});

        var countSql = "SELECT COUNT(*) FROM idx_notes WHERE UserId = @userId AND MATCH(@query)";
        var total = await _client.Ado.GetIntAsync(countSql, new { userId, query });

        var pageData = new PageData<NoteDto>
        {
            DataList = list,
            TotalCount = total,
            PageIndex = pageNumber,
            PageSize = pageSize
        };
        return pageData;
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

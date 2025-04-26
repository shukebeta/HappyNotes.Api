using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Framework.Extensions;
using HappyNotes.Dto;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using Api.Framework.Models;
using HappyNotes.Entities;

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
        var sql = "SELECT * FROM noteindex WHERE UserId = @userId AND MATCH(@query) LIMIT @offset, @pageSize";
        var list = await _client.Ado.SqlQueryAsync<NoteDto>(sql, new {userId, query, offset, pageSize});

        var countSql = "SELECT COUNT(*) FROM noteindex WHERE UserId = @userId AND MATCH(@query)";
        var total = await _client.Ado.GetIntAsync(countSql, new {userId, query});

        var pageData = new PageData<NoteDto>
        {
            DataList = list,
            TotalCount = total,
            PageIndex = pageNumber,
            PageSize = pageSize
        };
        return pageData;
    }

    public async Task SyncNoteToIndexAsync(Note note, string fullContent)
    {
        await _client.Ado.ExecuteCommandAsync(
            "REPLACE INTO noteindex (Id, UserId, IsLong, IsPrivate, IsMarkdown, Content, CreatedAt, UpdatedAt, DeletedAt) VALUES (@id, @userId, @isLong, @isPrivate, @isMarkdown, @content, @createdAt, @updatedAt, @deletedAt)",
            new
            {
                note.Id, note.UserId,
                isLong = note.IsLong ? 1 : 0,
                isPrivate = note.IsPrivate ? 1 : 0,
                isMarkdown = note.IsMarkdown ? 1 : 0,
                content = fullContent,
                note.CreatedAt,
                updatedAt = note.UpdatedAt ?? 0,
                deletedAt = note.DeletedAt ?? 0,
            });
    }

    public async Task DeleteNoteFromIndexAsync(long id)
    {
        await _client.Ado.ExecuteCommandAsync(
            "UPDATE noteindex SET deletedat = @timestamp WHERE Id = @id AND deletedat = 0", new
            {
                timestamp = DateTime.Now.ToUnixTimeSeconds(),
                id
            });
    }
}

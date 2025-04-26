using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Framework.Extensions;
using HappyNotes.Common.Enums;
using HappyNotes.Dto;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using Api.Framework.Models;
using HappyNotes.Entities;

namespace HappyNotes.Services;

public class SearchService : ISearchService
{
    private readonly IDatabaseClient _client;

    public SearchService(IDatabaseClient client)
    {
        _client = client;
    }

    public async Task<PageData<NoteDto>> SearchNotesAsync(long userId, string query, int pageNumber, int pageSize, NoteFilterType filter = NoteFilterType.Normal)
    {
        var countSql = filter == NoteFilterType.Normal
            ? "SELECT COUNT(*) FROM noteindex WHERE UserId = @userId AND MATCH(@query) AND DeletedAt = 0"
            : "SELECT COUNT(*) FROM noteindex WHERE UserId = @userId AND MATCH(@query) AND DeletedAt > 0";
        var total = await _client.GetIntAsync(countSql, new { userId, query });

        var offset = (pageNumber - 1) * pageSize;
        var sql = filter == NoteFilterType.Normal
            ? "SELECT * FROM noteindex WHERE UserId = @userId AND MATCH(@query) AND DeletedAt = 0 LIMIT @offset, @pageSize"
            : "SELECT * FROM noteindex WHERE UserId = @userId AND MATCH(@query) AND DeletedAt > 0 LIMIT @offset, @pageSize";
        var list = await _client.SqlQueryAsync<NoteDto>(sql, new { userId, query, offset, pageSize });

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
        await _client.ExecuteCommandAsync(
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
        await _client.ExecuteCommandAsync(
            "UPDATE noteindex SET deletedat = @timestamp WHERE Id = @id AND deletedat = 0", new
            {
                timestamp = DateTime.Now.ToUnixTimeSeconds(),
                id
            });
    }

    public async Task UndeleteNoteFromIndexAsync(long id)
    {
        await _client.ExecuteCommandAsync(
            "UPDATE noteindex SET deletedat = 0 WHERE Id = @id", new
            {
                id
            });
    }
public async Task PurgeDeletedNotesFromIndexAsync()
    {
        await _client.ExecuteCommandAsync("DELETE FROM noteindex WHERE deletedAt > 0", new { });
    }
}

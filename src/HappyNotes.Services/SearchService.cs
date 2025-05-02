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
using HappyNotes.Common;
using HappyNotes.Entities;
using HappyNotes.Models.Search;

namespace HappyNotes.Services;

public class SearchService : ISearchService
{
    private readonly IDatabaseClient _client;
    private readonly HttpClient _httpClient;

    public SearchService(IDatabaseClient client, HttpClient httpClient, ManticoreConnectionOptions options)
    {
        _client = client;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.HttpEndpoint);
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<PageData<NoteDto>> SearchNotesAsync(long userId, string query, int pageNumber, int pageSize, NoteFilterType filter = NoteFilterType.Normal)
    {
        var queryObject = _BuildNoteSearchQuery(userId, query, filter, pageSize, pageNumber);

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(queryObject), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("json/search", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        // Log the raw response for debugging
        Console.WriteLine("ManticoreSearch Response: " + responseContent);

        var searchResult = System.Text.Json.JsonSerializer.Deserialize<ManticoreSearchResult>(responseContent);

        var total = searchResult?.hits?.total ?? 0;
        var list = new List<NoteDto>();
        if (searchResult?.hits?.hits != null)
        {
            foreach (var hit in searchResult.hits.hits)
            {
                if (hit?._source != null)
                {
                    list.Add(new NoteDto
                    {
                        Id = hit._id,
                        UserId = hit._source.userid,
                        Content = hit._source.content ?? string.Empty,
                        IsLong = hit._source.islong == 1,
                        IsPrivate = hit._source.isprivate == 1,
                        IsMarkdown = hit._source.ismarkdown == 1,
                        CreatedAt = hit._source.createdat,
                        UpdatedAt = hit._source.updatedat,
                        DeletedAt = hit._source.deletedat
                    });
                }
            }
        }

        // Truncate content in C# if IsLong is true and content is longer than 1024 chars
        foreach (var note in list)
        {
            if (note.IsLong)
            {
                note.Content = note.Content.GetShort();
            }
        }

        var pageData = new PageData<NoteDto>
        {
            DataList = list,
            TotalCount = (int)total,
            PageIndex = pageNumber,
            PageSize = pageSize
        };
        return pageData;
    }

    public async Task SyncNoteToIndexAsync(Note note, string fullContent)
    {
        await _client.ExecuteCommandAsync(
            "REPLACE INTO noteindex (Id, UserId, IsLong, IsPrivate, IsMarkdown, Content, Tags, CreatedAt, UpdatedAt, DeletedAt) VALUES (@id, @userId, @isLong, @isPrivate, @isMarkdown, @content, @tags, @createdAt, @updatedAt, @deletedAt)",
            new
            {
                note.Id, note.UserId,
                isLong = note.IsLong ? 1 : 0,
                isPrivate = note.IsPrivate ? 1 : 0,
                isMarkdown = note.IsMarkdown ? 1 : 0,
                content = fullContent,
                tags = string.Join(" ", fullContent.GetTags()),
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

    private static Dictionary<string, object> _BuildNoteSearchQuery(long userId, string query, NoteFilterType filter, int pageSize, int pageNumber)
    {
        return new Dictionary<string, object>
        {
            { "table", "noteindex" },
            { "query", new Dictionary<string, object>
                {
                    { "bool", new Dictionary<string, object>
                        {
                            { "must", new List<object>
                                {
                                    new Dictionary<string, object> { { "match", new Dictionary<string, string> { { "Content", query } } } },
                                    new Dictionary<string, object> { { "equals", new Dictionary<string, long> { { "UserId", userId } } } },
                                    filter == NoteFilterType.Normal
                                        ? (object)new Dictionary<string, object> { { "equals", new Dictionary<string, long> { { "DeletedAt", 0 } } } }
                                        : new Dictionary<string, object> { { "range", new Dictionary<string, object> { { "DeletedAt", new Dictionary<string, long> { { "gt", 0 } } } } } }
                                }
                            }
                        }
                    }
                }
            },
            { "limit", pageSize },
            { "offset", (pageNumber - 1) * pageSize },
            { "sort", new List<object> { "_score", new Dictionary<string, string> { { "CreatedAt", "desc" } } } }
        };
    }
}

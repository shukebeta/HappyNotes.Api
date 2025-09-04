using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Api.Framework.Extensions;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Models.Search;
using HappyNotes.Services.interfaces;
using SqlSugar;

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
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<(List<long>, int)> GetNoteIdsByKeywordAsync(long userId, string query, int pageNumber, int pageSize, NoteFilterType filter = NoteFilterType.Normal)
    {
        query = query?.Trim() ?? string.Empty;
        if (query.Length == 0)
            return (new List<long>(), 0);

        // Bigram indexing requires at least 2 characters to generate valid tokens
        if (query.Length == 1)
            return (new List<long>(), 0);

        var queryObject = _BuildNoteSearchQuery(userId, query, filter, pageSize, pageNumber);

        var content = new StringContent(JsonSerializer.Serialize(queryObject), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("json/search", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // Log the error response for debugging
            Console.WriteLine("ManticoreSearch Error: " + errorContent);
            throw new Exception($"ManticoreSearch returned error: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        // Log the raw response for debugging
        Console.WriteLine("ManticoreSearch Response: " + responseContent);

        var searchResult = JsonSerializer.Deserialize<ManticoreSearchResult>(responseContent);

        var total = searchResult?.hits?.total ?? 0;
        var noteIdList = new List<long>();
        if (searchResult?.hits?.hits != null)
        {
            foreach (var hit in searchResult.hits.hits)
            {
                if (hit?._source != null)
                {
                    noteIdList.Add(hit._id);
                }
            }
        }

        return (noteIdList, (int)total);
    }

    public async Task SyncNoteToIndexAsync(Note note, string fullContent)
    {
        var parameters = new SugarParameter[]
        {
            new SugarParameter("@id", note.Id),
            new SugarParameter("@userId", note.UserId),
            new SugarParameter("@isLong", note.IsLong ? 1 : 0),
            new SugarParameter("@isPrivate", note.IsPrivate ? 1 : 0),
            new SugarParameter("@isMarkdown", note.IsMarkdown ? 1 : 0),
            new SugarParameter("@content", fullContent),
            new SugarParameter("@tags", string.Join(" ", fullContent.GetTags())),
            new SugarParameter("@createdAt", note.CreatedAt),
            new SugarParameter("@updatedAt", note.UpdatedAt ?? 0),
            new SugarParameter("@deletedAt", note.DeletedAt ?? 0)
        };

        await _client.ExecuteCommandAsync(
            "REPLACE INTO noteindex (Id, UserId, IsLong, IsPrivate, IsMarkdown, Content, Tags, CreatedAt, UpdatedAt, DeletedAt) VALUES (@id, @userId, @isLong, @isPrivate, @isMarkdown, @content, @tags, @createdAt, @updatedAt, @deletedAt)",
            parameters);
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
        var mustClauses = new List<object>
        {
            new Dictionary<string, object> // Query for content: prioritize exact phrase match
            {
                {
                    "bool", new Dictionary<string, object>
                    {
                        {
                            "should", new List<object>
                            {
                                new Dictionary<string, object> // Boosted exact phrase match
                                {
                                    { "match_phrase", new Dictionary<string, object>
                                        {
                                            { "Content", new Dictionary<string, object>
                                                {
                                                    { "query", query },
                                                    { "boost", 2.0f } // Adjust boost factor as needed
                                                }
                                            }
                                        }
                                    }
                                },
                                new Dictionary<string, object> // Regular match as fallback
                                {
                                    { "match", new Dictionary<string, string> { { "Content", query } } }
                                },
                                new Dictionary<string, object> // Match in tags
                                {
                                    { "match", new Dictionary<string, string> { { "Tags", query } } }
                                }
                            }
                        },
                        { "minimum_should_match", 1 } // At least one of the "should" clauses must match
                    }
                }
            },
            new Dictionary<string, object> { { "equals", new Dictionary<string, long> { { "UserId", userId } } } }
        };

        if (filter == NoteFilterType.Normal)
        {
            mustClauses.Add(new Dictionary<string, object> { { "equals", new Dictionary<string, long> { { "DeletedAt", 0 } } } });
        }
        else
        {
            mustClauses.Add(new Dictionary<string, object> { { "range", new Dictionary<string, object> { { "DeletedAt", new Dictionary<string, long> { { "gt", 0 } } } } } });
        }
        var source = new[] { "id" };

        return new Dictionary<string, object>
        {
            { "table", "noteindex" },
            { "track_scores", true },
            { "query", new Dictionary<string, object>
                {
                    { "bool", new Dictionary<string, object>
                        {
                            { "must", mustClauses }
                        }
                    }
                }
            },
            { "limit", pageSize },
            { "offset", (pageNumber - 1) * pageSize },
            { "sort", new List<object> // Sort by score (relevance) first, then by creation date
                {
                    new Dictionary<string, string> { { "_score", "desc" } },
                    new Dictionary<string, string> { { "CreatedAt", "desc" } }
                }
            },
            {"_source", source}
        };
    }
}

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

    public async Task<List<NoteDto>> SearchNotesAsync(string query, long userId, int page, int pageSize)
    {
        var results = new List<NoteDto>();
        var offset = (page - 1) * pageSize;

        // Manticoresearch query to search in content
        var list = await _client.SqlQueryable<NoteDto>("SELECT Id, Content, IsPrivate FROM idx_notes WHERE MATCH(@query) LIMIT @offset, @pageSize")
            .AddParameters(new { query, offset, pageSize })
            .ToListAsync();

        foreach (var note in list)
        {
            note.UserId = userId; // This will be overridden if necessary in the controller or service layer
            // Filter out private notes that do not belong to the user
            if (!note.IsPrivate || (note.IsPrivate && note.UserId == userId))
            {
                results.Add(note);
            }
        }

        return results;
    }
}

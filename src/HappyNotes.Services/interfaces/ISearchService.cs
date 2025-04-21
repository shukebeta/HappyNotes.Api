using System.Collections.Generic;
using System.Threading.Tasks;
using HappyNotes.Dto;

namespace HappyNotes.Services.interfaces;

public interface ISearchService
{
    Task<List<NoteDto>> SearchNotesAsync(string query, long userId, int page, int pageSize);
}

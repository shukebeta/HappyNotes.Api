using System.Collections.Generic;
using System.Threading.Tasks;
using HappyNotes.Dto;
using Api.Framework.Models;
using HappyNotes.Entities;

namespace HappyNotes.Services.interfaces;

public interface ISearchService
{
    Task<PageData<NoteDto>> SearchNotesAsync(long userId, string query, int pageNumber, int pageSize);
    Task SyncNoteToIndexAsync(Note note, string fullContent);
    Task DeleteNoteFromIndexAsync(long id);
    Task UndeleteNoteFromIndexAsync(long id);
}

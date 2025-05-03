using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Dto;
using HappyNotes.Entities;

namespace HappyNotes.Services.interfaces;

public interface ISearchService
{
    Task<PageData<NoteDto>> SearchNotesAsync(long userId, string query, int pageNumber, int pageSize, NoteFilterType filter = NoteFilterType.Normal);
    Task SyncNoteToIndexAsync(Note note, string fullContent);
    Task DeleteNoteFromIndexAsync(long id);
    Task UndeleteNoteFromIndexAsync(long id);
    Task PurgeDeletedNotesFromIndexAsync();
}


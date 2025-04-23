using System.Collections.Generic;
using System.Threading.Tasks;
using HappyNotes.Dto;
using Api.Framework.Models;

namespace HappyNotes.Services.interfaces;

public interface ISearchService
{
    Task<PageData<NoteDto>> SearchNotesAsync(long userId, string query, int pageNumber, int pageSize);
    Task SyncNoteToIndexAsync(long id, long userId, bool isPrivate, string content, long createdAt, long? updatedAt);
    Task DeleteNoteFromIndexAsync(long id);
}

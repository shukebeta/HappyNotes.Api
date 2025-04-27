using Api.Framework.Models;
using Api.Framework.Result;
using HappyNotes.Common.Enums;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Dto;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class NotesController(IMapper mapper
    , INoteService noteService
    , ICurrentUser currentUser
    , ISearchService searchService
): BaseController
{
    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    [EnforcePageSizeLimit(Constants.MaxPageSize)]
    public async Task<ApiResult<PageData<NoteDto>>> MyLatest(int pageSize, int pageNumber)
    {
        var notes = await noteService.GetUserNotes(currentUser.Id, pageSize, pageNumber, true);
        return new SuccessfulResult<PageData<NoteDto>>(mapper.Map<PageData<NoteDto>>(notes));
    }

    [AllowAnonymous]
    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    [EnforcePageSizeLimit(Constants.MaxPageSize)]
    public async Task<ApiResult<PageData<NoteDto>>> Latest(int pageSize, int pageNumber)
    {
        var notes = await noteService.GetPublicNotes(pageSize, pageNumber);
        return new SuccessfulResult<PageData<NoteDto>>(mapper.Map<PageData<NoteDto>>(notes));
    }

    [HttpGet("{noteId}")]
    public async Task<ApiResult<PageData<NoteDto>>> LinkedNotes(long noteId)
    {
        var notes = await noteService.GetLinkedNotes(currentUser.Id, noteId);
        return new SuccessfulResult<PageData<NoteDto>>(mapper.Map<PageData<NoteDto>>(notes));
    }

    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    public async Task<ApiResult<PageData<NoteDto>>> Tag(int pageSize, int pageNumber, string tag)
    {
        var notes = await noteService.GetUserTagNotes(currentUser.Id, pageSize, pageNumber, tag);
        return new SuccessfulResult<PageData<NoteDto>>(mapper.Map<PageData<NoteDto>>(notes));
    }

    [HttpGet]
    public async Task<ApiResult<List<NoteDto>>> Memories(string localTimezone)
    {
        var notes = await noteService.Memories(currentUser.Id, localTimezone);
        return new SuccessfulResult<List<NoteDto>>(mapper.Map<List<NoteDto>>(notes));
    }

    [HttpGet]
    public async Task<ApiResult<List<NoteDto>>> MemoriesOn(string localTimezone, string yyyyMMdd)
    {
        var notes = await noteService.MemoriesOn(currentUser.Id, localTimezone, yyyyMMdd);
        return new SuccessfulResult<List<NoteDto>>(mapper.Map<List<NoteDto>>(notes));
    }

    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    public async Task<ApiResult<PageData<NoteDto>>> LatestDeleted(int pageSize, int pageNumber)
    {
        var notes = await noteService.GetUserDeletedNotes(currentUser.Id, pageSize, pageNumber);
        return new SuccessfulResult<PageData<NoteDto>>(mapper.Map<PageData<NoteDto>>(notes));
    }

    [HttpDelete]
    public async Task<ApiResult> PurgeDeleted()
    {
        await noteService.PurgeUserDeletedNotes(currentUser.Id);
        return new SuccessfulResult<object>(null); // Indicate success with no specific data
    }

    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    public async Task<ApiResult<PageData<NoteDto>>> Search(string query, int pageNumber = 1, int pageSize = 10, string filter = "normal")
    {
        var filterType = filter.ToLower() == "deleted" ? NoteFilterType.Deleted : NoteFilterType.Normal;
        var results = await searchService.SearchNotesAsync(currentUser.Id, query, pageNumber, pageSize, filterType);
        return new SuccessfulResult<PageData<NoteDto>>(results);
    }
}

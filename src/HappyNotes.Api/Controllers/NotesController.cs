using Api.Framework;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class NotesController(IMapper mapper
    , CurrentUser currentUser
    , INoteService noteService
    , IRepositoryBase<Note> noteRepository
    , IRepositoryBase<LongNote> longNoteRepository
): BaseController
{
    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    [EnforcePageSizeLimit(Constants.MaxPageSize)]
    public async Task<ApiResult<PageData<NoteDto>>> MyLatest(int pageSize, int pageNumber)
    {
        var notes = await noteService.MyLatest(pageSize, pageNumber);
        return new SuccessfulResult<PageData<NoteDto>>(notes);
    }

    [HttpGet]
    public async Task<ApiResult<List<NoteDto>>> Memories(string localTimezone)
    {
        var notes = await noteService.Memories(localTimezone);
        return new SuccessfulResult<List<NoteDto>>(notes);
    }

    [HttpGet]
    public async Task<ApiResult<List<NoteDto>>> MemoriesIn(string localTimezone, string yyyyMMdd)
    {
        var notes = await noteService.MemoriesIn(localTimezone, yyyyMMdd);
        return new SuccessfulResult<List<NoteDto>>(notes);
    }

    [AllowAnonymous]
    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    [EnforcePageSizeLimit(Constants.MaxPageSize)]
    public async Task<ApiResult<PageData<NoteDto>>> Latest(int pageSize, int pageNumber)
    {
        var notes = await noteService.Latest(pageSize, pageNumber);
        return new SuccessfulResult<PageData<NoteDto>>(notes);
    }
}

using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HappyNotes.Api.Controllers;

public class NotesController(IMapper mapper
    , CurrentUser currentUser
    , INoteService noteService
    , IRepositoryBase<Note> noteRepository
    , IRepositoryBase<LongNote> longNoteRepository
): BaseController
{
    [HttpPost]
    public async Task<ApiResult<long>> Post(PostNoteRequest request)
    {
        var noteId = await noteService.Post(request);
        return new SuccessfulResult<long>(noteId);
    }

    [HttpPost("{noteId:long}")]
    public async Task<ApiResult> Update(long noteId, PostNoteRequest request)
    {
        await noteService.Update(noteId, request);
        return new SuccessfulResult<long>(noteId);
    }

    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    [EnforcePageSizeLimit(Constants.MaxPageSize)]
    public async Task<PageData<NoteDto>> MyLatest(int pageSize, int pageNumber)
    {
        var notes = await noteService.MyLatest(pageSize, pageNumber);
        return notes;
    }

    [AllowAnonymous]
    [HttpGet("{pageSize:int}/{pageNumber:int}")]
    [EnforcePageSizeLimit(Constants.MaxPageSize)]
    public async Task<PageData<NoteDto>> Latest(int pageSize, int pageNumber)
    {
        var notes = await noteService.Latest(pageSize, pageNumber);
        return notes;
    }
}

using Api.Framework;
using Api.Framework.Helper;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeihanLi.Extensions;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class NoteController(IMapper mapper
    , CurrentUser currentUser
    , INoteService noteService
    , IRepositoryBase<Note> noteRepository
    , IRepositoryBase<LongNote> longNoteRepository
): BaseController
{
    [HttpGet("{noteId}")]
    public async Task<ApiResult<Note>> Get(int noteId)
    {
        var note = await noteService.Get(noteId);
        return new SuccessfulResult<Note>(note!);
    }

    [AllowAnonymous]
    [HttpDelete("{noteId}")]
    public async Task<ApiResult<long>> Delete(int noteId)
    {
        var note = await noteService.Delete(noteId);
        return new SuccessfulResult<long>(noteId);
    }

    [HttpPost("{noteId}")]
    public async Task<ApiResult<long>> Undelete(int noteId)
    {
        var note = await noteService.Undelete(noteId);
        return new SuccessfulResult<long>(noteId);
    }

    [HttpPost]
    public async Task<ApiResult<long>> Post(PostNoteRequest request)
    {
        if (DuplicateRequestChecker.IsDuplicate(currentUser.Id, request))
        {
            // Return a response indicating that the request was ignored due to duplication
            throw ExceptionHelper.New(0, EventId._00105_DetectedDuplicatePostRequest);
        }
        var noteId = await noteService.Post(request);
        return new SuccessfulResult<long>(noteId);
    }

    [HttpPost("{noteId:long}")]
    public async Task<ApiResult> Update(long noteId, PostNoteRequest request)
    {
        if (DuplicateRequestChecker.IsDuplicate(currentUser.Id, request))
        {
            // Return a response indicating that the request was ignored due to duplication
            throw ExceptionHelper.New(currentUser.Id, EventId._00105_DetectedDuplicatePostRequest);
        }
        await noteService.Update(noteId, request);
        return new SuccessfulResult<long>(noteId);
    }

}

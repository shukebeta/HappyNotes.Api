using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common.Enums;
using HappyNotes.Dto;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class NoteController(IMapper mapper
    , ICurrentUser currentUser
    , INoteService noteService
): BaseController
{
    [HttpGet("{noteId}")]
    public async Task<ApiResult<NoteDto>> Get(int noteId, bool includeDeleted = false)
    {
        var note = await noteService.Get(currentUser.Id, noteId, includeDeleted);
        return new SuccessfulResult<NoteDto>(mapper.Map<NoteDto>(note));
    }

    [HttpDelete("{noteId}")]
    public async Task<ApiResult<long>> Delete(int noteId)
    {
        await noteService.Delete(currentUser.Id, noteId);
        return new SuccessfulResult<long>(noteId);
    }

    [HttpPost("{noteId}")]
    public async Task<ApiResult<long>> Undelete(int noteId)
    {
        await noteService.Undelete(currentUser.Id, noteId);
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
        var noteId = await noteService.Post(currentUser.Id, request);
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
        await noteService.Update(currentUser.Id, noteId, request);
        return new SuccessfulResult<long>(noteId);
    }

}

using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Dto;
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
) : BaseController
{
    [HttpGet("{noteId:long}")]
    public async Task<ApiResult<NoteDto>> Get(long noteId)
    {
        var note = await noteService.Get(currentUser.Id, noteId);
        return new SuccessfulResult<NoteDto>(mapper.Map<NoteDto>(note));
    }

    [HttpDelete("{noteId:long}")]
    public async Task<ApiResult<long>> Delete(long noteId)
    {
        await noteService.Delete(currentUser.Id, noteId);
        return new SuccessfulResult<long>(noteId);
    }

    /// <summary>
    /// Undeletes a previously deleted note.
    /// </summary>
    /// <param name="noteId">The ID of the note to undelete.</param>
    /// <returns>An ApiResult indicating success or failure.</returns>
    [HttpPost("{noteId:long}")]
    public async Task<ApiResult<long>> Undelete(long noteId)
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
            throw ExceptionHelper.New(EventId._00105_DetectedDuplicatePostRequest);
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

    [HttpPost("{noteId:long}/privacy")]
    public async Task<ApiResult<long>> SetIsPrivate(long noteId, SetNotePrivacyRequest request)
    {
        var result = await noteService.SetIsPrivate(currentUser.Id, noteId, request.IsPrivate);
        return result ? new SuccessfulResult<long>(noteId) : new FailedResult<long>(noteId, "0 rows updated");
    }
}

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
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NoteV2Controller(
    IMapper mapper,
    ICurrentUser currentUser,
    INoteService noteService
) : ControllerBase
{
    [HttpPost]
    public async Task<ApiResult<NoteDto>> Create(PostNoteRequest request)
    {
        if (DuplicateRequestChecker.IsDuplicate(currentUser.Id, request))
        {
            throw ExceptionHelper.New(EventId._00105_DetectedDuplicatePostRequest);
        }

        var noteId = await noteService.Post(currentUser.Id, request);
        var note = await noteService.Get(currentUser.Id, noteId);
        return new SuccessfulResult<NoteDto>(mapper.Map<NoteDto>(note));
    }

    [HttpPut("{noteId:long}")]
    public async Task<ApiResult<NoteDto>> Update(long noteId, PostNoteRequest request)
    {
        if (DuplicateRequestChecker.IsDuplicate(currentUser.Id, request))
        {
            throw ExceptionHelper.New(currentUser.Id, EventId._00105_DetectedDuplicatePostRequest);
        }

        await noteService.Update(currentUser.Id, noteId, request);
        var note = await noteService.Get(currentUser.Id, noteId, true);
        return new SuccessfulResult<NoteDto>(mapper.Map<NoteDto>(note));
    }
}

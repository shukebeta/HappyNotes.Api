using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
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
}

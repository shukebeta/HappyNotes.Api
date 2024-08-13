using Api.Framework.Result;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class TagController(CurrentUser currentUser
    , INoteTagService noteTagService
): BaseController
{
    [HttpGet]
    public async Task<ApiResult<List<TagCount>>> MyTagCloud()
    {
        var tagCountList = await noteTagService.GetTagData(currentUser.Id);
        return new SuccessfulResult<List<TagCount>>(tagCountList);
    }
}

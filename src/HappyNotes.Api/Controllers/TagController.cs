using Api.Framework.Result;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class TagController(ICurrentUser currentUser
    , INoteTagService noteTagService
) : BaseController
{
    [HttpGet]
    public async Task<ApiResult<List<TagCount>>> MyTagCloud(int limit = 80)
    {
        var tagCountList = await noteTagService.GetTagData(currentUser.Id, limit);
        return new SuccessfulResult<List<TagCount>>(tagCountList);
    }
}

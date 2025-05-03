using Api.Framework.Result;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    protected static ApiResult Fail(string message)
    {
        return new FailedResult<object?>(null, message);
    }

    protected static ApiResult Success(string? message = null)
    {
        return new SuccessfulResult<object?>(null, message);
    }
}

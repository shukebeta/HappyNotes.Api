using Api.Framework.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    protected ApiResult<T> Fail<T>(string message, T? data = default)
    {
        return new FailedResult<T>(data, message);
    }

    protected ApiResult<bool> Fail(string message)
    {
        return new FailedResult<bool>(false, message);
    }

    protected ApiResult<bool> Success(string? message = null)
    {
        return new SuccessfulResult<bool>(true, message);
    }
    protected ApiResult<T> Success<T>(T data)
    {
        return new SuccessfulResult<T>(data);
    }
}

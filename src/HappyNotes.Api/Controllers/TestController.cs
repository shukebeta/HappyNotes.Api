using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyNotes.Api.Controllers;

/// <summary>
/// Used for testing only!
/// </summary>
public class TestController(ILogger<TestController> logger) : BaseController
{
    [HttpGet]
    public ApiResult<string> ServiceStatus()
    {
        logger.LogInformation("test logger functionality: Information");
        return ResultHelper.New("hello");
    }
 
    [HttpGet]
    public ApiResult SampleFailure()
    {
        logger.LogError("test logger functionality: Error");
        return ResultHelper.New(1, "sample failure");
    }
 
    [HttpGet]
    public ApiResult<JwtToken> SampleCustomException()
    {
        throw CustomExceptionHelper.New(new JwtToken {Token="hello"}, "test custom exception with data");
    }
}

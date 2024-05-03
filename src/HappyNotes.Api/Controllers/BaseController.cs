using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    protected string RealIp => GetRealIp();

    private string GetRealIp()
    {
        return Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
    }

}
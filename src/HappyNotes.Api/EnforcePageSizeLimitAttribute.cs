namespace HappyNotes.Api;

using Microsoft.AspNetCore.Mvc.Filters;

public class EnforcePageSizeLimitAttribute(int maxPageSize) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("pageSize", out var pageSizeObj) && pageSizeObj is int pageSize)
        {
            // Enforce the maximum page size limit
            if (pageSize > maxPageSize)
            {
                context.ActionArguments["pageSize"] = maxPageSize;
            }
        }

        base.OnActionExecuting(context);
    }
}

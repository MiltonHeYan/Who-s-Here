using Microsoft.AspNetCore.Mvc.Filters;

namespace WhosHereServer.Filters
{
    public class AutoMapDtoActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}

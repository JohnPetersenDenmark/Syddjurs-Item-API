using Microsoft.AspNetCore.Mvc.Filters;
using Syddjurs_Item_API.Interfaces;
using System.Security.Claims;

namespace Syddjurs_Item_API.Services
{
    public class ResolveUserClaimsFilter : IActionFilter
    {
        private readonly IUserContext _userContext;

        public ResolveUserClaimsFilter(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated == true)
            {              
                _userContext.UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _userContext.Email = user.FindFirst(ClaimTypes.Email)?.Value;
                _userContext.UserName = user.FindFirst(ClaimTypes.Name)?.Value;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}

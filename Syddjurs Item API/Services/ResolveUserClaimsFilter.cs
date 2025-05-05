using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Syddjurs_Item_API.Interfaces;
using Syddjurs_Item_API.Models;
using System.Security.Claims;

namespace Syddjurs_Item_API.Services
{
    public class ResolveUserClaimsFilter : IActionFilter
    {
        private readonly IUserContext _userContext;
        private UserManager<ApplicationUser> _userManager;

        public ResolveUserClaimsFilter(IUserContext userContext, UserManager<ApplicationUser> userManager)
        {
            _userContext = userContext;
            _userManager = userManager;
        }       
        public void OnActionExecuted(ActionExecutedContext context) { }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpUser = context.HttpContext.User;

            if (httpUser.Identity?.IsAuthenticated == true)
            {
                var userId = httpUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var applicationUser = _userManager.FindByIdAsync(userId).Result;
                if ((applicationUser != null))
                {
                    _userContext.CurrentUser = applicationUser;
                    //_userContext.Email = applicationUser.Email;
                    //_userContext.UserId = applicationUser.Id;



                    //_userContext.Email = applicationUser.FindFirst(ClaimTypes.Email)?.Value;
                    //_userContext.UserName = user.FindFirst(ClaimTypes.Name)?.Value;
                }

            }
        }
    }
}

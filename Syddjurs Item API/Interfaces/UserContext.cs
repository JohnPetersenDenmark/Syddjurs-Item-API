using Syddjurs_Item_API.Models;
using System.Security.Claims;

namespace Syddjurs_Item_API.Interfaces
{
    public interface IUserContext
    {
        public ApplicationUser CurrentUser { get; set; }

        //string? UserId { get; set; }
        //string? UserName { get; set; }
        //string? Email { get; set; }
        //List<Claim> UserRoles { get; set; }
    }
}

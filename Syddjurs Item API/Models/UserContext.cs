using Syddjurs_Item_API.Interfaces;

namespace Syddjurs_Item_API.Models
{
    public class UserContext : IUserContext
    {
        //public string? UserId { get; set; }
        //public string? UserName { get; set; }
        //public string? Email { get; set; }        
        public ApplicationUser CurrentUser { get; set; }
    }
}

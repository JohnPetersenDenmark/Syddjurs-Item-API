using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class UserDto
    {
        [JsonPropertyName("id")]
        public string Id{ get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }


        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("roles")]
        public List<RoleDto> Roles { get; set; }

        public UserDto()
        {
               Roles = new List<RoleDto>(); 
        }
    }
}

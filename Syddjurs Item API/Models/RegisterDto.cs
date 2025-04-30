using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class RegisterDto
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}

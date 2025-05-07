using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class RoleDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }


        [JsonPropertyName("roleName")]
        public string RoleName { get; set; }

        [JsonPropertyName("isCheckBoxChecked")]
        public bool IsCheckBoxChecked { get; set; }
        

    }
}

using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class ItemCategoryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }
    }
}

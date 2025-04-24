using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class ItemListDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("number")]
        public int? Number { get; set; }

        [JsonPropertyName("lendable")]
        public bool? Lendable { get; set; }
    }
}

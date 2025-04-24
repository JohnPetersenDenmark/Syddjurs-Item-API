using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class Item
    {
  [JsonPropertyName("id")]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public string Description { get; set; }

        public string Sex { get; set; }

        public int Number { get; set; }

        public bool Lendable { get; set; }     
        public string Size { get; set; }     
        public ItemCategory Categori { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class LoanItemLine
    {
        public int Id { get; set; }     
        public int ItemId { get; set; }
        public int LoanId { get; set; }
        public string? Note { get; set; }      
        public int Number { get; set; }
    }
}

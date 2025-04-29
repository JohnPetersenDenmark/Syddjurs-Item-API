using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class Loan
    {
      
        public int Id { get; set; }

        public string Lender { get; set; }
      
        public string LoanDate { get; set; }
    }
}

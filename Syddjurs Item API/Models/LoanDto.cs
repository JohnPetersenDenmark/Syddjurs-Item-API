using Syddjurs.Models;
using System.Text.Json.Serialization;

namespace Syddjurs_Item_API.Models
{
    public class LoanDto
    {
        public LoanDto()
        {
            LoanItemLines = new List<LoanItemLinesUploadDto>();
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("lender")]
        public string Lender { get; set; }

        [JsonPropertyName("loanDate")]
        public string LoanDate { get; set; }

        [JsonPropertyName("loanitemlines")]
        public List<LoanItemLinesUploadDto>? LoanItemLines { get; set; }
    }
}


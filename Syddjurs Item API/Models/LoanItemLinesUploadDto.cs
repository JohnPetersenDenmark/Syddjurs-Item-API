using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Syddjurs.Models
{
    public class LoanItemLinesUploadDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("itemId")]
        public int ItemId { get; set; }

        [JsonPropertyName("loanId")]
        public int LoanId { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }
    }
}

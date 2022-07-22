using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncomeTaxCalculator.Models.Entities
{
    public class tax_files
    {
        [Key]
        public int file_id { get; set; }

        public string PAN { get; set; }

        public string income_split_up { get; set; }

        public string exemptions { get; set; }

        public string deductions { get; set; }

        public string regime { get; set; }
    }
}

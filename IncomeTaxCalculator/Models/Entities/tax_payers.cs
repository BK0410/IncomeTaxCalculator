using System.ComponentModel.DataAnnotations;

namespace IncomeTaxCalculator.Models.Entities
{
    public class tax_payers
    {

        [Key]
        public int tax_payer_id { get; set; }

        public string Username { get; set; }

        public string PAN { get; set; }

        public string Email_ID { get; set; }

        public string Password { get; set; }
    }
}

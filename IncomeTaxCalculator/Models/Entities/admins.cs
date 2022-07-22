using System.ComponentModel.DataAnnotations;

namespace IncomeTaxCalculator.Models.Entities
{
    public class admins
    {
        [Key]
        public int Admin_id { get; set; }

        public string Username { get; set; }

        public string Email_ID { get; set; }

        public string Password { get; set; }
    }
}

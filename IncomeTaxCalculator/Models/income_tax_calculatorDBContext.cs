using IncomeTaxCalculator.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncomeTaxCalculator.Models
{
    public class income_tax_calculatorDBContext:DbContext
    {
        public income_tax_calculatorDBContext(DbContextOptions<income_tax_calculatorDBContext> options) : base(options)
        {

        }
        public DbSet<tax_payers> tax_payers { get; set; }

        public DbSet<admins> admins { get; set; }

        public DbSet<tax_files> tax_files { get; set; }
        public DbSet<final_report> final_report { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace IncomeTaxCalculator.Models.Entities
{
    public class final_report
    {
        [Key]
        public int report_id { get; set; }
        public string PAN { get; set; }
        public string Name_AsPerPAN { get; set; }
        public string income_split_up { get; set; }
        public double total_income { get; set; }
        public string exemptions_applied { get; set; }
        public string exemptions_approved { get; set; }
        public double gross_taxable_income { get; set; }
        public string deductions_applied { get; set; }
        public string deductions_approved { get; set; }
        public double net_taxable_income { get; set; }
        public string regime { get; set; }
        public string tax_slab { get; set; }
        public double net_income_tax { get; set; }
        public double cess { get; set; }
        public double total_income_tax { get; set; }

    }
}

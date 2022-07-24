using IncomeTaxCalculator.Models;
using IncomeTaxCalculator.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IncomeTaxCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {

        private readonly income_tax_calculatorDBContext _income_tax_calculatorDbContext;
        private readonly IConfiguration _configuration;

        public AdminController(income_tax_calculatorDBContext income_tax_calculatorDbContext, IConfiguration configuration)
        {
            _income_tax_calculatorDbContext = income_tax_calculatorDbContext;
            _configuration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var users = await _income_tax_calculatorDbContext.admins.ToListAsync();
            return Ok(users);
        }

        [HttpGet]
        [Route("getTaxPayers")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetTaxPayers()
        {
            var tax_payers = await _income_tax_calculatorDbContext.tax_payers.ToListAsync();
            return Ok(tax_payers);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("taxPayer/{ID}")]
        public async Task<IActionResult> GetTaxPayerByID(int ID)
        {
            var tax_payer = await _income_tax_calculatorDbContext.tax_payers.FindAsync(ID);
            return Ok(tax_payer);
        }

        [HttpGet]
        [Route("getTaxFiles")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetTaxFiles()
        {
            var tax_files = await _income_tax_calculatorDbContext.tax_files.ToListAsync();
            Console.WriteLine("__________________________________________________________________");
            Console.WriteLine(tax_files.First());
            return Ok(tax_files);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("taxFile/{ID}")]
        public async Task<IActionResult> GetTaxFileByID(int ID)
        {
            string a = "{\"barath\":210000.00,\"sanjai\":100000.06}";
            JObject json = JObject.Parse(a);
            var sum = 0.00;
            foreach (var item in json)
            {
                Console.WriteLine("]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]");
                
                if(item.Value != null)
                {
                    double totalDatas = (double)item.Value.Value<double>();
                    sum = sum + totalDatas;
                }
                
                Console.WriteLine(sum);
            }
            

            var tax_file = await _income_tax_calculatorDbContext.tax_files.FindAsync(ID);          
            var exemptions = tax_file.exemptions;
            JObject json1 = JObject.Parse(exemptions);
            Console.WriteLine(json1["HRA"]);
            
            return Ok(tax_file);


        }
        [HttpPost]
        [Route("adminlogin")]
        public async Task<IActionResult> Login([FromBody] Login details)
        {
            var admin = await _income_tax_calculatorDbContext.admins.FirstOrDefaultAsync(x => x.Email_ID == details.Email_ID && x.Password == details.Password);
            if (admin != null)
            {
                string token = CreateToken(details);
                return Ok(token);
            }

            else
            {
                return BadRequest("Invalid Email_ID or Password");
            }

        }

        private string CreateToken(Login details)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role,"admin"),
                new Claim(ClaimTypes.Email,details.Email_ID)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("reportpreparation")]
        public async Task<IActionResult> submitReport()
        {
            Console.WriteLine("Enter PAN: ");
            string PAN = Console.ReadLine();
            var tax_file = _income_tax_calculatorDbContext.tax_files.FirstOrDefault(x => x.PAN == PAN);
            if (tax_file != null)
            {
                string Name_AsPerPAN = tax_file.Name_AsPerPAN;
                Console.WriteLine("Income Split-UP: ");
                Console.WriteLine(tax_file.income_split_up);
                

                JObject json = JObject.Parse(tax_file.income_split_up);
                var total_income = 0.00;
                foreach (var item in json)
                {
                    if (item.Value != null)
                    {
                        double income = (double)item.Value.Value<double>();
                        total_income = total_income + income;
                    }
                    
                }
                Console.WriteLine("========================================================================");
                Console.WriteLine("Total Income: Rs. " + total_income);

                if (tax_file.regime == "old")
                {
                    Console.WriteLine("Exemptions: ");
                    Console.WriteLine(tax_file.exemptions);
                    Console.WriteLine("Enter Approved Exemptions: ");
                    string exemptions_approved = Console.ReadLine();

                    JObject json1 = JObject.Parse(exemptions_approved);
                    var total_exemptions = 0.00;
                    foreach (var item in json1)
                    {
                        if (item.Value != null)
                        {
                            double exemption = (double)item.Value.Value<double>();
                            total_exemptions = total_exemptions + exemption;
                        }
                        
                    }
                    Console.WriteLine("========================================================================");
                    Console.WriteLine("Total Exemption: Rs. " + total_exemptions);
                    double gross_taxable_income = total_income - total_exemptions;
                    Console.WriteLine("Gross Taxable Income: Rs. " + gross_taxable_income);

                    Console.WriteLine("Deductions Applied: " + tax_file.deductions);
                    Console.WriteLine("Enter Approved Deductions: ");
                    string deductions_approved = Console.ReadLine();

                    JObject json2 = JObject.Parse(deductions_approved);
                    var total_deductions = 0.00;
                    foreach (var item in json2)
                    {
                        if (item.Value != null)
                        {
                            double deduction = (double)item.Value.Value<double>();
                            total_deductions = total_deductions + deduction;
                        }
                    }
                    Console.WriteLine("========================================================================");
                    Console.WriteLine("Total Deduction: Rs. " + total_deductions);
                    double net_taxable_income = total_income - total_exemptions - total_deductions;
                    Console.WriteLine("Net Taxable Income: Rs. " + net_taxable_income);

                    //Tax Slabs

                    Console.WriteLine("\nTax Slab 1 : Rs.0 to Rs. 2,50,000\nTax Slab 2 : Rs.2,50,000 to Rs.5,00,000\nTax Slab 3 : Rs.5,00,000 to Rs.10,00,000\nTax Slab 4 : > Rs.10,00,000");

                    double tax_slab_1 = 0;
                    double tax_slab_2 = 0;
                    double tax_slab_3 = 0;
                    double tax_slab_4 = 0;

                    if (net_taxable_income <= 250000)
                    {
                        tax_slab_1 = net_taxable_income;
                    }
                    else if (250000 <= net_taxable_income && net_taxable_income <= 500000)
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = net_taxable_income - 250000;
                    }
                    else if (500000 <= net_taxable_income && net_taxable_income <= 1000000)
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = 250000;
                        tax_slab_3 = net_taxable_income - 500000;
                    }
                    else
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = 250000;
                        tax_slab_3 = 500000;
                        tax_slab_4 = net_taxable_income - 1000000;
                    }

                    Console.WriteLine("===========================================================");
                    Console.WriteLine("Tax Slab of " + tax_file.Name_AsPerPAN);
                    Console.WriteLine("\nTax Slab 1: " + tax_slab_1 + " x 0 % = " + "0" );
                    Console.WriteLine("Tax Slab 2: " + tax_slab_2 + " x 5 % = " + (tax_slab_2 * 0.05));
                    Console.WriteLine("Tax Slab 3: " + tax_slab_3 + " x 20 % = " + (tax_slab_3 * 0.2));
                    Console.WriteLine("Tax Slab 4: " + tax_slab_4 + " x 30 % = " + (tax_slab_4 * 0.3));

                    var tax_slab = new { tax_slab_1 = 0, tax_slab_2 = (tax_slab_2 * 0.05), tax_slab_3 = (tax_slab_3 * 0.2), tax_slab_4 = (tax_slab_4 * 0.3) };

                    double net_income_tax = 0 + (tax_slab_2 * 0.05) + (tax_slab_3 * 0.2) + (tax_slab_4 * 0.3);
                    Console.WriteLine("Net Income Tax: Rs. " + net_income_tax);
                    double cess = net_income_tax * 0.04;
                    Console.WriteLine("CESS Amount: Rs. " + cess);
                    double total_income_tax = net_income_tax + cess;
                    Console.WriteLine("Total Income Tax: Rs. " + total_income_tax);
                    await _income_tax_calculatorDbContext.SaveChangesAsync();
                    return Ok(new {
                        PAN = PAN,
                        Name_AsPerPAN = Name_AsPerPAN,
                        income_split_up = tax_file.income_split_up,
                        total_income = total_income,
                        exemptions_applied = tax_file.exemptions,
                        exemptions_approved = exemptions_approved,
                        gross_taxable_income = gross_taxable_income,
                        deductions_applied = tax_file.deductions,
                        deductions_approved = deductions_approved,
                        net_taxable_income = net_taxable_income,
                        regime = tax_file.regime,
                        tax_slab = tax_slab,
                        net_income_tax = net_income_tax,
                        cess = cess,
                        Total_income_tax = total_income_tax
                    });
                }
                else
                {
                    string exemptions_approved = "0";
                    double gross_taxable_income = total_income;
                    string deductions_approved = "0";
                    double net_taxable_income = total_income;

                    // Tax SLab

                    Console.WriteLine("\nTax Slab 1 : Rs.0 to Rs. 2,50,000\nTax Slab 2 : Rs.2,50,000 to Rs.5,00,000\nTax Slab 3 : Rs.5,00,000 to Rs.7,50,000\nTax Slab 4 : Rs.7,50,000 to Rs.10,00,000\nTax Slab 5 : Rs.10,00,000 to Rs. 12,50,000\nTax Slab 6 : Rs.12,50,000 to Rs. 15,00,000\nTax Slab 7 : > Rs. 15,00,000");

                    double tax_slab_1 = 0;
                    double tax_slab_2 = 0;
                    double tax_slab_3 = 0;
                    double tax_slab_4 = 0;
                    double tax_slab_5 = 0;
                    double tax_slab_6 = 0;
                    double tax_slab_7 = 0;

                    if (net_taxable_income <= 250000)
                    {
                        tax_slab_1 = net_taxable_income;
                    }
                    else if (250000 <= net_taxable_income && net_taxable_income <= 500000)
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = net_taxable_income - 250000;
                    }
                    else if (500000 <= net_taxable_income && net_taxable_income <= 750000)
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = 250000;
                        tax_slab_3 = net_taxable_income - 250000;
                    }
                    else if (750000 <= net_taxable_income && net_taxable_income <= 1000000)
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = 250000;
                        tax_slab_3 = 250000;
                        tax_slab_4 = net_taxable_income - 250000;
                    }
                    else if (1000000 <= net_taxable_income && net_taxable_income <= 1250000)
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = 250000;
                        tax_slab_3 = 250000;
                        tax_slab_4 = 250000;
                        tax_slab_5 = net_taxable_income - 250000;
                    }
                    else if (1250000 <= net_taxable_income && net_taxable_income <= 1500000)
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = 250000;
                        tax_slab_3 = 250000;
                        tax_slab_4 = 250000;
                        tax_slab_5 = 250000;
                        tax_slab_6 = net_taxable_income - 250000;
                    }
                    else
                    {
                        tax_slab_1 = 250000;
                        tax_slab_2 = 250000;
                        tax_slab_3 = 250000;
                        tax_slab_4 = 250000;
                        tax_slab_5 = 250000;
                        tax_slab_6 = 250000;
                        tax_slab_7 = net_taxable_income - 1500000;
                    }

                    Console.WriteLine("===========================================================");
                    Console.WriteLine("Tax Slab of " + tax_file.Name_AsPerPAN);
                    Console.WriteLine("\nTax Slab 1: " + tax_slab_1 + " x 0 % = " + "0");
                    Console.WriteLine("Tax Slab 2: " + tax_slab_2 + " x 5 % = " + (tax_slab_2 * 0.05));
                    Console.WriteLine("Tax Slab 3: " + tax_slab_3 + " x 10 % = " + (tax_slab_3 * 0.1));
                    Console.WriteLine("Tax Slab 4: " + tax_slab_4 + " x 15 % = " + (tax_slab_4 * 0.15));
                    Console.WriteLine("Tax Slab 5: " + tax_slab_5 + " x 20 % = " + (tax_slab_5 * 0.2));
                    Console.WriteLine("Tax Slab 6: " + tax_slab_6 + " x 25 % = " + (tax_slab_6 * 0.25));
                    Console.WriteLine("Tax Slab 7: " + tax_slab_7 + " x 30 % = " + (tax_slab_7 * 0.3));

                    var tax_slab = new { tax_slab_1 = 0, tax_slab_2 = (tax_slab_2 * 0.05), tax_slab_3 = (tax_slab_3 * 0.1), tax_slab_4 = (tax_slab_4 * 0.15), tax_slab_5 = (tax_slab_5 * 0.2), tax_slab_6 = (tax_slab_6 * 0.25), tax_slab_7 = (tax_slab_7 * 0.3) };

                    double net_income_tax = 0 + (tax_slab_2 * 0.05) + (tax_slab_3 * 0.1) + (tax_slab_4 * 0.15) + (tax_slab_5 * 0.2) + (tax_slab_6 * 0.25) + (tax_slab_7 * 0.3);
                    Console.WriteLine("Net Income Tax: Rs. " + net_income_tax);
                    double cess = net_income_tax * 0.04;
                    Console.WriteLine("CESS Amount: Rs. " + cess);
                    double total_income_tax = net_income_tax + cess;
                    Console.WriteLine("Total Income Tax: Rs. " + total_income_tax);

                    return Ok(new {
                        PAN = PAN,
                        Name_AsPerPAN = Name_AsPerPAN,
                        income_split_up = tax_file.income_split_up,
                        total_income = total_income,
                        exemptions_applied = tax_file.exemptions,
                        exemptions_approved = exemptions_approved,
                        gross_taxable_income = gross_taxable_income,
                        deductions_applied = tax_file.deductions,
                        deductions_approved = deductions_approved,
                        net_taxable_income = net_taxable_income,
                        regime = tax_file.regime,
                        tax_slab = tax_slab,
                        net_income_tax = net_income_tax,
                        cess = cess,
                        Total_income_tax = total_income_tax
                    });
                }
                
            }
            Console.WriteLine("=================== File Not Found ===================");
            return BadRequest("File Not Found");
        }

        [HttpPost]
        [Route("reportsubmission")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> submitReport([FromBody]final_report newReport)
        {
            _income_tax_calculatorDbContext.final_report.Add(newReport);
            await _income_tax_calculatorDbContext.SaveChangesAsync();
            return Ok("Report Submitted Successfully");
        }


    }
}

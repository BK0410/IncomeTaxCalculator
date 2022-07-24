using IncomeTaxCalculator.Models;
using IncomeTaxCalculator.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Wkhtmltopdf.NetCore;

namespace IncomeTaxCalculator.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TaxPayerController : ControllerBase
    {

        private readonly income_tax_calculatorDBContext _income_tax_calculatorDbContext;
        private readonly IConfiguration _configuration;
        readonly IGeneratePdf _generatePdf;

        public TaxPayerController(IGeneratePdf generatePdf,income_tax_calculatorDBContext income_tax_calculatorDbContext, IConfiguration configuration)
        {
            _income_tax_calculatorDbContext = income_tax_calculatorDbContext;
            _configuration = configuration;
            _generatePdf = generatePdf;
        }



        [HttpGet]
        [Authorize(Roles = "tax_payer")]
        public async Task<IActionResult> GetAsync()
        {
            var tax_payers = await _income_tax_calculatorDbContext.tax_payers.ToListAsync();
            return Ok(tax_payers);
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup(tax_payers newAccount)
        {
            _income_tax_calculatorDbContext.tax_payers.Add(newAccount);
            await _income_tax_calculatorDbContext.SaveChangesAsync();
            return Ok("Account Created Successfully");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Login details)
        {
            var tax_payer = await _income_tax_calculatorDbContext.tax_payers.FirstOrDefaultAsync(x => x.Email_ID == details.Email_ID && x.Password == details.Password);
            if (tax_payer != null)
            {

                string token = CreateToken(details,tax_payer.PAN);
                return Ok(token);
            }

            else
            {
                return BadRequest("Invalid Email_ID or Password");
            }

        }

        private string CreateToken(Login details,string PAN)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.SerialNumber,PAN),
                new Claim(ClaimTypes.Role,"tax_payer"),
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

        [HttpPost]
        [Authorize(Roles = "tax_payer")]
        [Route("taxfiling")]
        public async Task<IActionResult> taxFiling(tax_files newFile)
        {
            _income_tax_calculatorDbContext.tax_files.Add(newFile);
            await _income_tax_calculatorDbContext.SaveChangesAsync();

            string authorization = Request.Headers["Authorization"];
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                string jwttoken = authorization.Substring("Bearer ".Length).Trim();
                var jwt = jwttoken;
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var role = token.Claims.First().Value;
                if (role == "123456789")
                {
                    Console.WriteLine("|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
                }
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine(role);
            }


            var details = await _income_tax_calculatorDbContext.tax_files.FirstOrDefaultAsync(x => x.PAN == newFile.PAN);
            return Ok("Filing Process Succesful");
        }


        [HttpGet]
        [Authorize(Roles = "tax_payer")]
        [Route("myfiledetails")]
        public async Task<IActionResult> getMyFileDetails()
        {
            string authorization = Request.Headers["Authorization"];
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                string jwttoken = authorization.Substring("Bearer ".Length).Trim();
                var jwt = jwttoken;
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var PAN = token.Claims.First().Value;
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine(PAN);
                var file_details = await _income_tax_calculatorDbContext.tax_files.FirstOrDefaultAsync(x => x.PAN == PAN);
                return Ok(file_details);
            }
            else
            {
                return BadRequest("No Files Found");
            }
            
        }

        [HttpGet]
        [Authorize(Roles = "tax_payer")]
        [Route("get")]
        public async Task<IActionResult> GetTaxReport()
        {

            string authorization = Request.Headers["Authorization"];
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                string jwttoken = authorization.Substring("Bearer ".Length).Trim();
                var jwt = jwttoken;
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var PAN = token.Claims.First().Value;
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine(PAN);
                var report = await _income_tax_calculatorDbContext.final_report.FirstOrDefaultAsync(x => x.PAN == PAN);
                if(report != null) 
                {
                    return await _generatePdf.GetPdf("Views/TaxPayer/TaxReport.cshtml", report);
                }
                else
                {
                    return BadRequest("Report Not Found yet...!");
                }
            }
            else
            {
                return BadRequest("Some error has been found.");
            }
                
        }

        [HttpGet]
        [Authorize(Roles = "tax_payer")]
        [Route("authcheck")]
        public async Task<IActionResult> authCheck()
        {
            return Ok("Authorization Successful");
        }

    }

    public class Login
    {
        public string Email_ID { get; set; }

        public string Password { get; set; }
    }
}

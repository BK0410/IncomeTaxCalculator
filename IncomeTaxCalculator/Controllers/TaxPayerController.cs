using IncomeTaxCalculator.Models;
using IncomeTaxCalculator.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IncomeTaxCalculator.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TaxPayerController : ControllerBase
    {

        private readonly income_tax_calculatorDBContext _income_tax_calculatorDbContext;
        private readonly IConfiguration _configuration;

        public TaxPayerController(income_tax_calculatorDBContext income_tax_calculatorDbContext, IConfiguration configuration)
        {
            _income_tax_calculatorDbContext = income_tax_calculatorDbContext;
            _configuration = configuration;
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
                new Claim(ClaimTypes.Role,"tax_payer")
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
        [Authorize(Roles = "tax_payer")]
        [Route("signout")]
        public async Task<IActionResult> Signout()
        {
            return Ok("Signed Out Successfully");
        }

    }

    public class Login
    {
        public string Email_ID { get; set; }

        public string Password { get; set; }
    }
}

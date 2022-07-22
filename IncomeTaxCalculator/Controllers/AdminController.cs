using IncomeTaxCalculator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IncomeTaxCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {

        private readonly income_tax_calculatorDBContext _income_tax_calculatorDbContext;

        public AdminController(income_tax_calculatorDBContext income_tax_calculatorDbContext)
        {
            _income_tax_calculatorDbContext = income_tax_calculatorDbContext;
        }


        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var users = await _income_tax_calculatorDbContext.admins.ToListAsync();
            return Ok(users);
        }
    }
}

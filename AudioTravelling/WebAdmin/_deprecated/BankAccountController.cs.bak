using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _service;

        public BankAccountController(IBankAccountService service)
        {
            _service = service;
        }

        // GET: api/BankAccount
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankAccount>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET: api/BankAccount/ABC123
        [HttpGet("{id}")]
        public async Task<ActionResult<BankAccount>> GetById(string id)
        {
            var bankAccount = await _service.GetByIdAsync(id);
            if (bankAccount == null) return NotFound();
            return Ok(bankAccount);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _service;

        public StoreController(IStoreService service)
        {
            _service = service;
        }

        // GET: api/Store
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Store>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET: api/Store/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Store>> GetById(int id)
        {
            var store = await _service.GetByIdAsync(id);
            if (store == null) return NotFound();
            return Ok(store);
        }
    }
}
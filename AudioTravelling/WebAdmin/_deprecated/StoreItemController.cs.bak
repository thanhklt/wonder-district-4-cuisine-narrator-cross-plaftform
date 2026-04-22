using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreItemController : ControllerBase
    {
        private readonly IStoreItemService _service;

        public StoreItemController(IStoreItemService service)
        {
            _service = service;
        }

        // GET: api/StoreItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreItem>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET: api/StoreItem/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreItem>> GetById(int id)
        {
            var storeItem = await _service.GetByIdAsync(id);
            if (storeItem == null) return NotFound();
            return Ok(storeItem);
        }
    }
}
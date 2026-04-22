using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GalleryController : ControllerBase
    {
        private readonly IGalleryService _service;

        public GalleryController(IGalleryService service)
        {
            _service = service;
        }

        // GET: api/Gallery
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gallery>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET: api/Gallery/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Gallery>> GetById(int id)
        {
            var gallery = await _service.GetByIdAsync(id);
            if (gallery == null) return NotFound();
            return Ok(gallery);
        }
    }
}
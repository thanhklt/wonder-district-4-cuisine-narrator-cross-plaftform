using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _service;

        public PackageController(IPackageService service)
        {
            _service = service;
        }

        // GET: api/Package
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Package>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET: api/Package/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Package>> GetById(int id)
        {
            var package = await _service.GetByIdAsync(id);
            if (package == null) return NotFound();
            return Ok(package);
        }
    }
}
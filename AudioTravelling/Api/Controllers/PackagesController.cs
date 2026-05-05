using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/packages")]
    [ApiController]
    [Authorize] // Can be called by Owner or Admin
    public class PackagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PackagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var packages = await _context.Packages.Select(p => new PackageDto
            {
                PackageId = p.PackageId,
                Name = p.Name,
                Radius = p.Radius,
                Priority = p.Priority,
                Price = p.Price
            }).ToListAsync();

            return Ok(packages);
        }
    }

    [Route("api/admin/packages")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPackagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminPackagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var packages = await _context.Packages.Select(p => new PackageDto
            {
                PackageId = p.PackageId,
                Name = p.Name,
                Radius = p.Radius,
                Priority = p.Priority,
                Price = p.Price
            }).ToListAsync();

            return Ok(packages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Packages.FindAsync(id);
            if (p == null) return NotFound();

            return Ok(new PackageDto
            {
                PackageId = p.PackageId,
                Name = p.Name,
                Radius = p.Radius,
                Priority = p.Priority,
                Price = p.Price
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PackageDto request)
        {
            var package = new Package
            {
                Name = request.Name,
                Radius = request.Radius,
                Priority = request.Priority,
                Price = request.Price
            };

            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Created successfully", packageId = package.PackageId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PackageDto request)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null) return NotFound();

            package.Name = request.Name;
            package.Radius = request.Radius;
            package.Priority = request.Priority;
            package.Price = request.Price;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null) return NotFound();

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }
    }

    public class PackageDto
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Radius { get; set; }
        public int Priority { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

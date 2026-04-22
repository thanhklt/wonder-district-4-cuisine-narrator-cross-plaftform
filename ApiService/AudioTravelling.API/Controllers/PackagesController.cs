using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/packages")]
public class PackagesController(IAppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var packages = await db.Packages
            .OrderBy(p => p.Priority)
            .Select(p => new { p.Id, p.Name, p.RadiusMeters, p.Priority, p.Price, p.Description })
            .ToListAsync();
        return Ok(packages);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] PackageRequest req)
    {
        var package = new Core.Entities.Package
        {
            Name = req.Name,
            RadiusMeters = req.RadiusMeters,
            Priority = req.Priority,
            Price = req.Price,
            Description = req.Description,
        };
        db.Packages.Add(package);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = package.Id }, package);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] PackageRequest req)
    {
        var package = await db.Packages.FindAsync(id);
        if (package is null) return NotFound();
        package.Name = req.Name;
        package.RadiusMeters = req.RadiusMeters;
        package.Priority = req.Priority;
        package.Price = req.Price;
        package.Description = req.Description;
        await db.SaveChangesAsync();
        return Ok(package);
    }

    public record PackageRequest(string Name, int RadiusMeters, int Priority, decimal Price, string Description);
}

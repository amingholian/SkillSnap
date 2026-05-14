using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
  private readonly SkillSnapContext _context;
  private readonly IMemoryCache _cache;

  public ProjectsController(SkillSnapContext context, IMemoryCache cache)
  {
    _context = context;
    _cache = cache;
  }

  private const string ProjectsCacheKey = "project_list";

  [HttpGet]
  public async Task<IActionResult> GetProjects()
  {
    if (!_cache.TryGetValue(ProjectsCacheKey, out List<Project>? projects))
    {
      projects = await _context.Projects
        .AsNoTracking()
        .Select(p => new Project
        {
          Id = p.Id,
          Title = p.Title,
          Description = p.Description,
          PortfolioUser = new PortfolioUser
          {
            Id = p.PortfolioUserId,
            Name = p.PortfolioUser != null ? p.PortfolioUser.Name : string.Empty
          }
        })
        .ToListAsync();

      var cacheOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(5));
      _cache.Set(ProjectsCacheKey, projects, cacheOptions);
    }
    return Ok(projects);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetProject(int id)
  {
    var project = await _context.Projects
        .AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new Project
        {
          Id = p.Id,
          Title = p.Title,
          Description = p.Description,
          ImageUrl = p.ImageUrl,
          PortfolioUserId = p.PortfolioUserId,
          PortfolioUser = new PortfolioUser
          {
            Id = p.PortfolioUserId,
            Name = p.PortfolioUser != null ? p.PortfolioUser.Name : string.Empty
          }
        })
        .FirstOrDefaultAsync();

    if (project == null) return NotFound();
    return Ok(project);
  }

  [Authorize]
  [HttpPost]
  public async Task<ActionResult<Project>> AddProject([FromBody] Project project)
  {
    if (project.PortfolioUserId <= 0)
      return BadRequest("A valid PortfolioUserId is required.");

    var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == project.PortfolioUserId);
    if (!userExists)
      return BadRequest($"PortfolioUser with Id {project.PortfolioUserId} does not exist.");

    _context.Projects.Add(project);
    await _context.SaveChangesAsync();
    _cache.Remove(ProjectsCacheKey);
    return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
  }

  [Authorize(Roles = "Admin")]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateProject(int id, [FromBody] Project project)
  {
    if (id != project.Id) return BadRequest();
    _context.Entry(project).State = EntityState.Modified;
    await _context.SaveChangesAsync();
    _cache.Remove(ProjectsCacheKey);
    return NoContent();
  }

  [Authorize(Roles = "Admin")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteProject(int id)
  {
    var project = await _context.Projects
        .FirstOrDefaultAsync(p => p.Id == id);
    if (project == null) return NotFound();
    _context.Projects.Remove(project);
    await _context.SaveChangesAsync();
    _cache.Remove(ProjectsCacheKey);
    return NoContent();
  }
}
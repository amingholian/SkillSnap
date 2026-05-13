using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
  private readonly SkillSnapContext _context;

  public ProjectsController(SkillSnapContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
  {
    return await _context.Projects.ToListAsync();
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Project>> GetProject(int id)
  {
    var project = await _context.Projects.FindAsync(id);
    if (project == null) return NotFound();
    return project;
  }

  [HttpPost]
  public async Task<ActionResult<Project>> AddProject([FromBody] Project project)
  {
    _context.Projects.Add(project);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateProject(int id, [FromBody] Project project)
  {
    if (id != project.Id) return BadRequest();
    _context.Entry(project).State = EntityState.Modified;
    await _context.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteProject(int id)
  {
    var project = await _context.Projects.FindAsync(id);
    if (project == null) return NotFound();
    _context.Projects.Remove(project);
    await _context.SaveChangesAsync();
    return NoContent();
  }
}
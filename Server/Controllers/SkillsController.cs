using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
  private readonly SkillSnapContext _context;

  public SkillsController(SkillSnapContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
  {
    return await _context.Skills
        .AsNoTracking()
        .ToListAsync();
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Skill>> GetSkill(int id)
  {
    var skill = await _context.Skills
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Id == id);
    if (skill == null) return NotFound();
    return skill;
  }

  [Authorize]
  [HttpPost]
  public async Task<ActionResult<Skill>> AddSkill([FromBody] Skill skill)
  {
    if (skill.PortfolioUserId <= 0)
      return BadRequest("A valid PortfolioUserId is required.");

    var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == skill.PortfolioUserId);
    if (!userExists)
      return BadRequest($"PortfolioUser with Id {skill.PortfolioUserId} does not exist.");

    _context.Skills.Add(skill);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
  }
}

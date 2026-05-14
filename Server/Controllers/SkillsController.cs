using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
  private readonly SkillSnapContext _context;
  private readonly IMemoryCache _cache;
  private const string SkillsCacheKey = "skill_list";

  public SkillsController(SkillSnapContext context, IMemoryCache cache)
  {
    _context = context;
    _cache = cache;
  }

  [HttpGet]
  public async Task<IActionResult> GetSkills()
  {
    if (!_cache.TryGetValue(SkillsCacheKey, out List<Skill>? skills))
    {
      skills = await _context.Skills
          .AsNoTracking()
          .ToListAsync();

      var cacheOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromMinutes(5));
      _cache.Set(SkillsCacheKey, skills, cacheOptions);
    }
    return Ok(skills);
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
    _cache.Remove(SkillsCacheKey);
    return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
  }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioUsersController : ControllerBase
{
  private readonly SkillSnapContext _context;

  public PortfolioUsersController(SkillSnapContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<PortfolioUser>>> GetUsers()
  {
    return await _context.PortfolioUsers
        .AsNoTracking()
        .Select(u => new PortfolioUser
        {
          Id = u.Id,
          Name = u.Name,
          Bio = u.Bio,
          ProfileImageUrl = u.ProfileImageUrl,
          Skills = u.Skills.Select(s => new Skill
          {
            Id = s.Id,
            Name = s.Name,
            Level = s.Level,
            PortfolioUserId = s.PortfolioUserId
          }).ToList(),
          Projects = u.Projects.Select(p => new Project
          {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            PortfolioUserId = p.PortfolioUserId
          }).ToList()
        })
        .ToListAsync();
  }
}

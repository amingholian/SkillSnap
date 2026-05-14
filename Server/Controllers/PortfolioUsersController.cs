using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioUsersController : ControllerBase
{
  private readonly SkillSnapContext _context;
  private readonly IMemoryCache _cache;
  private const string UsersCacheKey = "portfolio_user_list";

  public PortfolioUsersController(SkillSnapContext context, IMemoryCache cache)
  {
    _context = context;
    _cache = cache;
  }

  [HttpGet]
  public async Task<IActionResult> GetUsers()
  {
    if (!_cache.TryGetValue(UsersCacheKey, out List<PortfolioUser>? users))
    {
      users = await _context.PortfolioUsers
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

      var cacheOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromMinutes(5));
      _cache.Set(UsersCacheKey, users, cacheOptions);
    }
    return Ok(users);
  }
}

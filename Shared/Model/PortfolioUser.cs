using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Shared.Models;

public class PortfolioUser
{
  [Key]
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = string.Empty;

  public string? Bio { get; set; }
  public string? ProfileImageUrl { get; set; }

  public List<Project> Projects { get; set; } = new();
  public List<Skill> Skills { get; set; } = new();
}

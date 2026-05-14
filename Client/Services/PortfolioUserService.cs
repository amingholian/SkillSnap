using System.Net.Http.Json;
using SkillSnap.Shared.Models;

public class PortfolioUserService
{
  private readonly HttpClient _http;

  public PortfolioUserService(HttpClient http)
  {
    _http = http;
  }

  public async Task<List<PortfolioUser>> GetUsersAsync()
  {
    return await _http.GetFromJsonAsync<List<PortfolioUser>>("api/PortfolioUsers") ?? new List<PortfolioUser>();
  }
}

using System.Net.Http.Json;
using SkillSnap.Shared.Models;

public class SkillService
{
  private readonly HttpClient _http;

  public SkillService(HttpClient http)
  {
    _http = http;
  }

  public async Task<List<Skill>> GetSkillsAsync()
  {
    return await _http.GetFromJsonAsync<List<Skill>>("api/Skills") ?? new List<Skill>();
  }

  public async Task<(bool Success, string? Error)> AddSkillAsync(Skill newSkill)
  {
    var response = await _http.PostAsJsonAsync("api/Skills", newSkill);
    if (!response.IsSuccessStatusCode)
    {
      var body = await response.Content.ReadAsStringAsync();
      return (false, string.IsNullOrWhiteSpace(body) ? $"Error {(int)response.StatusCode}" : body);
    }
    return (true, null);
  }
}

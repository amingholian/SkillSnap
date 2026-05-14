using System.Net.Http.Json;
using SkillSnap.Shared.Models;

public class ProjectService
{
  private readonly HttpClient _http;

  public ProjectService(HttpClient http)
  {
    _http = http;
  }

  public async Task<List<Project>> GetProjectsAsync()
  {
    return await _http.GetFromJsonAsync<List<Project>>("api/Projects") ?? new List<Project>();
  }

  public async Task AddProjectAsync(Project newProject)
  {
    var response = await _http.PostAsJsonAsync("api/Projects", newProject);
    response.EnsureSuccessStatusCode();
  }
}

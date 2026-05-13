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
    return await _http.GetFromJsonAsync<List<Project>>("api/Projects");
  }

  public async Task AddProjectAsync(Project newProject)
  {
    await _http.PostAsJsonAsync("api/Projects", newProject);
  }
}

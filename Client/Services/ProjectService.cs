using System.Net.Http.Json;
using SkillSnap.Shared.Models;

public class ProjectService
{
  private readonly HttpClient _http;

  public ProjectService(HttpClient http)
  {
    _http = http;
  }

  /// <summary>Fetches all projects from the API.</summary>
  public async Task<List<Project>> GetProjectsAsync()
  {
    return await _http.GetFromJsonAsync<List<Project>>("api/Projects") ?? new List<Project>();
  }

  /// <summary>Posts a new project. Returns success flag and error message on failure.</summary>
  public async Task<(bool Success, string? Error)> AddProjectAsync(Project newProject)
  {
    var response = await _http.PostAsJsonAsync("api/Projects", newProject);
    if (!response.IsSuccessStatusCode)
    {
      var body = await response.Content.ReadAsStringAsync();
      return (false, string.IsNullOrWhiteSpace(body) ? $"Error {(int)response.StatusCode}" : body);
    }
    return (true, null);
  }
}

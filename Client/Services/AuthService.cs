using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SkillSnap.Shared.Models;

public class AuthService
{
  private const string TokenKey = "authToken";

  private readonly HttpClient _http;
  private readonly ILocalStorageService _localStorage;

  public AuthService(HttpClient http, ILocalStorageService localStorage)
  {
    _http = http;
    _localStorage = localStorage;
  }

  public async Task<string?> GetTokenAsync()
      => await _localStorage.GetItemAsync<string>(TokenKey);

  public async Task<bool> IsAuthenticatedAsync()
      => !string.IsNullOrEmpty(await GetTokenAsync());

  public event Action? AuthStateChanged;

  public async Task SetTokenAsync(string token)
  {
    await _localStorage.SetItemAsync(TokenKey, token);
    _http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
    AuthStateChanged?.Invoke();
  }

  public async Task LogoutAsync()
  {
    await _localStorage.RemoveItemAsync(TokenKey);
    _http.DefaultRequestHeaders.Authorization = null;
    AuthStateChanged?.Invoke();
  }

  public async Task InitializeAsync()
  {
    var token = await GetTokenAsync();
    if (!string.IsNullOrEmpty(token))
      _http.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", token);
  }

  public async Task<(bool Success, string? Error)> LoginAsync(LoginModel model)
  {
    var response = await _http.PostAsJsonAsync("api/auth/login", model);
    if (!response.IsSuccessStatusCode)
      return (false, "Invalid email or password.");

    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
    if (result?.Token == null)
      return (false, "Token not received.");

    await SetTokenAsync(result.Token);
    return (true, null);
  }

  public async Task<(bool Success, string? Error)> RegisterAsync(RegisterModel model)
  {
    var response = await _http.PostAsJsonAsync("api/auth/register", model);
    if (!response.IsSuccessStatusCode)
      return (false, "Registration failed. Check your details.");

    return (true, null);
  }

  private class TokenResponse
  {
    public string? Token { get; set; }
  }
}

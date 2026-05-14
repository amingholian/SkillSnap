using System.Security.Claims;
using System.Text.Json;

public class UserSessionService
{
  public string? UserId { get; private set; }
  public string? Email { get; private set; }
  public string? Role { get; private set; }
  public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);

  // Editing state — components can set/read this to persist across navigation
  public int? ActiveProjectId { get; set; }
  public int? ActiveSkillId { get; set; }

  public event Action? OnChange;

  public void LoadFromToken(string token)
  {
    var claims = ParseClaimsFromJwt(token);
    UserId = claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    Email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
    Role = claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value ?? "User";
    NotifyStateChanged();
  }

  public void Clear()
  {
    UserId = null;
    Email = null;
    Role = null;
    ActiveProjectId = null;
    ActiveSkillId = null;
    NotifyStateChanged();
  }

  private void NotifyStateChanged() => OnChange?.Invoke();

  private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
  {
    var parts = jwt.Split('.');
    if (parts.Length != 3)
      return Enumerable.Empty<Claim>();

    var payload = parts[1];

    // Re-pad base64url to standard base64
    payload = payload.Replace('-', '+').Replace('_', '/');
    payload = (payload.Length % 4) switch
    {
      2 => payload + "==",
      3 => payload + "=",
      _ => payload
    };

    var bytes = Convert.FromBase64String(payload);
    var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes);

    return dict?.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()))
           ?? Enumerable.Empty<Claim>();
  }
}

using System.Security.Claims;
using System.Text.Json;

/// <summary>
/// Scoped service that holds the currently authenticated user's session state,
/// parsed from the JWT token. Also stores transient editing state shared across components.
/// </summary>
public class UserSessionService
{
  /// <summary>Identity subject claim (ASP.NET Identity user ID).</summary>
  public string? UserId { get; private set; }
  /// <summary>Email address from the JWT email claim.</summary>
  public string? Email { get; private set; }
  /// <summary>Role from the JWT role claim. Defaults to "User" when not present.</summary>
  public string? Role { get; private set; }
  /// <summary>True when a user ID is present and the token has not expired.</summary>
  public bool IsAuthenticated => !string.IsNullOrEmpty(UserId) && !IsTokenExpired();
  private long _expUnix;

  // Editing state — components can set/read this to persist across navigation
  public int? ActiveProjectId { get; set; }
  public int? ActiveSkillId { get; set; }

  /// <summary>Raised whenever authentication or session state changes.</summary>
  public event Action? OnChange;

  /// <summary>Parses claims from <paramref name="token"/> and populates session state.</summary>
  public void LoadFromToken(string token)
  {
    var claims = ParseClaimsFromJwt(token);
    UserId = claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    Email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
    Role = claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value ?? "User";
    var expStr = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
    _expUnix = long.TryParse(expStr, out var exp) ? exp : 0;
    NotifyStateChanged();
  }

  /// <summary>Clears all session state (called on logout).</summary>
  public void Clear()
  {
    UserId = null;
    Email = null;
    Role = null;
    _expUnix = 0;
    ActiveProjectId = null;
    ActiveSkillId = null;
    NotifyStateChanged();
  }

  private bool IsTokenExpired()
  {
    if (_expUnix == 0) return true;
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    return now >= _expUnix;
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

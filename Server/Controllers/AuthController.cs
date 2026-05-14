using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly IConfiguration _config;

  public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _config = config;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterModel request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    if (request.Password != request.ConfirmPassword)
      return BadRequest(new { message = "Passwords do not match." });

    var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
    var result = await _userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
      return Ok(new { message = "User registered successfully." });

    return BadRequest(result.Errors);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginModel request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user == null)
      return Unauthorized(new { message = "Invalid email or password." });

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
    if (!result.Succeeded)
      return Unauthorized(new { message = "Invalid email or password." });

    var token = GenerateJwt(user);
    return Ok(new { token });
  }

  private string GenerateJwt(ApplicationUser user)
  {
    var keyStr = _config["Jwt:Key"];
    if (string.IsNullOrEmpty(keyStr))
      throw new InvalidOperationException("JWT key is not configured.");

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiryMinutes = double.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;
    var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id),
      new Claim(JwtRegisteredClaimNames.Email, user.Email!),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
      issuer: _config["Jwt:Issuer"],
      audience: _config["Jwt:Audience"],
      claims: claims,
      expires: expiry,
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}

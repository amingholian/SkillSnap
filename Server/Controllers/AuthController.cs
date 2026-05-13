using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;

  public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
  {
    _userManager = userManager;
    _signInManager = signInManager;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterModel request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
    var result = await _userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
      return Ok(new { message = "User registered successfully" });

    return BadRequest(result.Errors);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginModel request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

    if (result.Succeeded)
      return Ok(new { message = "User logged in successfully" });

    return BadRequest(new { message = "Invalid email or password" });
  }
}
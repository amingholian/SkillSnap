using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkillSnap.Server.Data;
using System.Text;

namespace SkillSnap.Server.Extensions;

public static class ServiceExtensions
{
  public static IServiceCollection AddSkillSnapData(this IServiceCollection services, IConfiguration config)
  {
    services.AddDbContext<SkillSnapContext>(options =>
      options.UseSqlite(config.GetConnectionString("Default")));
    services.AddMemoryCache();
    return services;
  }

  public static IServiceCollection AddSkillSnapAuth(this IServiceCollection services, IConfiguration config)
  {
    services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<SkillSnapContext>();

    var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
      };
    });

    return services;
  }

  public static IServiceCollection AddSkillSnapCors(this IServiceCollection services, IConfiguration config)
  {
    var origins = config.GetSection("CorsOrigins").Get<string[]>() ?? [];
    services.AddCors(options =>
    {
      options.AddPolicy("BlazorClient", policy =>
        policy.WithOrigins(origins)
          .AllowAnyHeader()
          .AllowAnyMethod());
    });
    return services;
  }
}

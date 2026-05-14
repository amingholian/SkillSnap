using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;

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

  public static IServiceCollection AddSkillSnapAuth(this IServiceCollection services)
  {
    services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<SkillSnapContext>();
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

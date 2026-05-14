using System.Diagnostics;

namespace SkillSnap.Server.Middleware;

public class RequestTimingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<RequestTimingMiddleware> _logger;

  public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var sw = Stopwatch.StartNew();

    context.Response.OnStarting(() =>
    {
      sw.Stop();
      context.Response.Headers["X-Response-Time-Ms"] = sw.ElapsedMilliseconds.ToString();
      return Task.CompletedTask;
    });

    await _next(context);

    _logger.LogInformation(
      "{Method} {Path} responded {StatusCode} in {Elapsed}ms",
      context.Request.Method,
      context.Request.Path,
      context.Response.StatusCode,
      sw.ElapsedMilliseconds);
  }
}

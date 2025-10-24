using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Context;
using System.Diagnostics;
using System.Text;

namespace Sales123.Sales.WebApi.Support
{
    public sealed class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly RequestMetrics? _metrics;

        private const int MaxBodyChars = 4000;

        private static readonly PathString[] SkipPaths =
        {
            "/swagger", "/favicon.ico", "/index.html"
        };

        private static readonly HashSet<string> HeavyMethods =
            new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            RequestMetrics? metrics = null)
        {
            _next = next;
            _logger = logger;
            _metrics = metrics;
        }

        public async Task Invoke(HttpContext ctx)
        {
          
            if (SkipPaths.Any(p => ctx.Request.Path.StartsWithSegments(p)))
            {
                await _next(ctx);
                return;
            }

            var method = ctx.Request.Method;

        
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await _next(ctx);
                }
                finally
                {
                    sw.Stop();
                    var status = ctx.Response.StatusCode;
                    var url = ctx.Request.GetDisplayUrl(); 
                    _logger.LogInformation(
                        "IN  {Method} {Url} -> {Status} in {Elapsed} ms",
                        method.ToUpperInvariant(),
                        url,
                        status,
                        sw.ElapsedMilliseconds
                    );
                    _metrics?.ObserveRequest(method, ctx.Request.Path, status, sw.ElapsedMilliseconds);
                }
                return;
            }

            if (!HeavyMethods.Contains(method))
            {
                await _next(ctx);
                return;
            }

            var swHeavy = Stopwatch.StartNew();

            var correlationId = ctx.Request.Headers["x-correlation-id"].FirstOrDefault()
                                ?? Guid.NewGuid().ToString("N");
            ctx.Response.Headers["x-correlation-id"] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                var reqBody = await ReadBody(ctx.Request);

                var original = ctx.Response.Body;
                await using var mem = new MemoryStream();
                ctx.Response.Body = mem;

                try
                {
                    await _next(ctx);
                }
                finally
                {
                    swHeavy.Stop();

                    mem.Position = 0;
                    var respText = await new StreamReader(mem).ReadToEndAsync();
                    mem.Position = 0;
                    await mem.CopyToAsync(original);
                    ctx.Response.Body = original;

                    var status = ctx.Response.StatusCode;

                    _logger.LogInformation(
                        "IN  {Method} {Path} -> {Status} in {Elapsed} ms | req: {Req} | resp: {Resp}",
                        method.ToUpperInvariant(),
                        ctx.Request.Path,
                        status,
                        swHeavy.ElapsedMilliseconds,
                        reqBody,
                        Truncate(respText)
                    );

                    _metrics?.ObserveRequest(method, ctx.Request.Path, status, swHeavy.ElapsedMilliseconds);
                }
            }
        }

        private static async Task<string> ReadBody(HttpRequest request)
        {
            if (!IsText(request.ContentType))
                return $"<non-text content: {request.ContentType ?? "unknown"}>";

            request.EnableBuffering();
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return Truncate(body);
        }

        private static bool IsText(string? ct) =>
            ct != null &&
            (ct.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
             ct.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
             ct.EndsWith("+json", StringComparison.OrdinalIgnoreCase));

        private static string Truncate(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Length <= MaxBodyChars ? s : s[..MaxBodyChars] + $"…(+{s.Length - MaxBodyChars} chars)";
        }
    }
}

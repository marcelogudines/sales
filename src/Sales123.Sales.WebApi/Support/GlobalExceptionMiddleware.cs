using MassTransit;
using Microsoft.AspNetCore.Http.Extensions; 
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.Infra.Messaging.MassTransit;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Sales123.Sales.WebApi.Support;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private const int MaxBodyChars = 4000;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var traceId = ctx.TraceIdentifier;
        var correlationId = ctx.Request.Headers["x-correlation-id"].FirstOrDefault();
        var status = (int)HttpStatusCode.InternalServerError;

        var method = ctx.Request.Method;
        var url = ctx.Request.GetDisplayUrl();
        var path = ctx.Request.Path + ctx.Request.QueryString;
        var ctype = ctx.Request.ContentType;
        var reqBody = await TryReadRequestBody(ctx.Request);

     
        var st = new StackTrace(ex, true);
        var (srcType, srcMethod, srcFile, srcLine) = ExtractRoot(st);
        var friendly = FormatStack(st, maxFrames: 12);

       
        _logger.LogError(ex,
            "ApiErrorOccurred: traceId={TraceId}, correlationId={CorrelationId}, {Method} {Url}, status={Status}, type={Type}, message={Message}, source={Source}@{File}:{Line}, ctype={ContentType}, req={Request}\n{FriendlyStack}",
            traceId, correlationId, method, url, status, ex.GetType().FullName, ex.Message,
            $"{srcType}.{srcMethod}", srcFile ?? "<n/a>", srcLine ?? 0, ctype ?? "<none>",
            reqBody ?? "<empty>", friendly);

       
        try
        {
            if (ctx.RequestServices.GetService<IPublishEndpoint>() is { } publish)
            {
                await publish.Publish(new ApiErrorOccurred(
                    TraceId: traceId,
                    CorrelationId: correlationId,
                    Method: method,
                    Url: url,
                    Path: path,
                    StatusCode: status,
                    Message: ex.Message,
                    ExceptionType: ex.GetType().FullName,
                    StackTrace: ex.ToString(),
                    RequestContentType: ctype,
                    RequestBody: reqBody,
                    SourceType: srcType,
                    SourceMethod: srcMethod,
                    SourceFile: srcFile,
                    SourceLine: srcLine,
                    FriendlyStack: friendly,
                    OccurredAt: DateTimeOffset.UtcNow
                ));
            }
        }
        catch (Exception busEx)
        {
            _logger.LogError(busEx, "Falha ao publicar ApiErrorOccurred (traceId={TraceId})", traceId);
        }

      
        if (!ctx.Response.HasStarted)
        {
            ctx.Response.Clear();
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";

            var bag = new NotificationsBag();
            bag.Add("internal_error", "Ocorreu um erro interno. Tente novamente mais tarde.", path: null);

            var payload = ApiResponse<object>.Error(bag, traceId);
            await ctx.Response.WriteAsJsonAsync(payload);
        }
    }

    private static (string? type, string? method, string? file, int? line) ExtractRoot(StackTrace st)
    {
     
        var frames = st.GetFrames() ?? Array.Empty<StackFrame>();
        var frame = frames.FirstOrDefault(f => !string.IsNullOrEmpty(f.GetFileName()))
                  ?? frames.FirstOrDefault();

        var method = frame?.GetMethod();
        var type = method?.DeclaringType?.FullName;
        var name = method?.Name;
        var file = frame?.GetFileName();
        var line = frame?.GetFileLineNumber();

      
        if (!string.IsNullOrEmpty(file))
            file = System.IO.Path.GetFileName(file);

        return (type, name, file, line == 0 ? null : line);
    }

    private static string FormatStack(StackTrace st, int maxFrames = 10)
    {
        var frames = st.GetFrames() ?? Array.Empty<StackFrame>();
        var take = frames.Take(maxFrames);

        var sb = new StringBuilder();
        foreach (var f in take)
        {
            var m = f.GetMethod();
            var type = m?.DeclaringType?.FullName ?? "?";
            var name = m?.Name ?? "?";
            var file = f.GetFileName();
            var line = f.GetFileLineNumber();

            if (!string.IsNullOrEmpty(file))
                file = System.IO.Path.GetFileName(file);

            sb.Append("  at ")
              .Append(type).Append('.').Append(name).Append("()");

            if (!string.IsNullOrEmpty(file) || line > 0)
                sb.Append(" in ").Append(file ?? "?").Append(':').Append(line);

            sb.AppendLine();
        }
        if (frames.Length > maxFrames)
            sb.Append("  ... ").Append(frames.Length - maxFrames).Append(" more frames").AppendLine();

        return sb.ToString();
    }

    private static async Task<string?> TryReadRequestBody(HttpRequest request)
    {
        if (!IsText(request.ContentType)) return null;

        try
        {
            request.EnableBuffering();
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body)) return null;
            return body.Length <= MaxBodyChars ? body : body[..MaxBodyChars] + $"…(+{body.Length - MaxBodyChars} chars)";
        }
        catch
        {
            return "<request body indisponível>";
        }
    }

    private static bool IsText(string? ct) =>
        ct != null &&
        (ct.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
         ct.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
         ct.EndsWith("+json", StringComparison.OrdinalIgnoreCase));
}

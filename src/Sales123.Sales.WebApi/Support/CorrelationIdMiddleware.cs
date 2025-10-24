using Serilog.Context;

namespace Sales123.Sales.WebApi.Support
{
    public sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string HeaderName = "x-correlation-id";

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            var cid = ctx.Request.Headers[HeaderName].FirstOrDefault()
                      ?? Guid.NewGuid().ToString("N");

            ctx.Request.Headers[HeaderName] = cid;
            ctx.Response.Headers[HeaderName] = cid;

            using (LogContext.PushProperty("CorrelationId", cid))
            {
                await _next(ctx);
            }
        }
    }
}

namespace Sales123.Sales.WebApi.Support;

public sealed class RequestBufferingMiddleware
{
    private readonly RequestDelegate _next;
    public RequestBufferingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
      
        if (HttpMethods.IsPost(ctx.Request.Method) ||
            HttpMethods.IsPut(ctx.Request.Method) ||
            HttpMethods.IsPatch(ctx.Request.Method) ||
            HttpMethods.IsDelete(ctx.Request.Method))
        {
            ctx.Request.EnableBuffering();
        }
        await _next(ctx);
    }
}

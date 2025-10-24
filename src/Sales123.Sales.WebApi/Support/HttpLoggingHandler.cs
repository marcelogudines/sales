using System.Diagnostics;

namespace Sales123.Sales.WebApi.Support
{

    public sealed class HttpLoggingHandler : DelegatingHandler
    {
        private readonly ILogger<HttpLoggingHandler> _logger;
        private const int MaxBodyChars = 4000;

        private static readonly HashSet<string> AllowedMethods =
            new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "POST", "PUT", "PATCH", "DELETE" };

        public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger) => _logger = logger;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (!AllowedMethods.Contains(request.Method.Method))
                return await base.SendAsync(request, ct);

            var sw = Stopwatch.StartNew();

            var isLight = request.Method == HttpMethod.Get || request.Method == HttpMethod.Head;
            string reqBody = isLight ? string.Empty : await ReadContentSafely(request.Content);

            var resp = await base.SendAsync(request, ct);

            string respBody = isLight ? string.Empty : await ReadContentSafely(resp.Content);

            sw.Stop();

            if (isLight)
            {
                _logger.LogInformation(
                    "OUT {Method} {Url} -> {Status} in {Elapsed} ms",
                    request.Method.Method.ToUpperInvariant(),
                    request.RequestUri,
                    (int)resp.StatusCode,
                    sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "OUT {Method} {Url} -> {Status} in {Elapsed} ms | req: {Req} | resp: {Resp}",
                    request.Method.Method.ToUpperInvariant(),
                    request.RequestUri,
                    (int)resp.StatusCode,
                    sw.ElapsedMilliseconds,
                    reqBody,
                    respBody);
            }

            return resp;
        }

        private static async Task<string> ReadContentSafely(HttpContent? content)
        {
            if (content == null) return string.Empty;

            var ct = content.Headers.ContentType?.MediaType ?? "";
            var isText = string.IsNullOrEmpty(ct)
                         || ct.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
                         || ct.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                         || ct.EndsWith("+json", StringComparison.OrdinalIgnoreCase);

            if (!isText) return $"<non-text content: {ct}>";

            var text = await content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(text)) return string.Empty;

            return text.Length <= MaxBodyChars ? text : text[..MaxBodyChars] + $"…(+{text.Length - MaxBodyChars} chars)";
        }
    }
}

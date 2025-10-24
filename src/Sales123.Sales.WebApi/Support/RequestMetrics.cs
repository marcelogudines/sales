using Microsoft.AspNetCore.Http;

namespace Sales123.Sales.WebApi.Support
{
  
    public sealed class RequestMetrics
    {
        public const string ActivitySourceName = "Sales123.Sales.WebApi";
        public const string MeterName = "Sales123.Sales.WebApi";

        private long _totalRequests;
        private long _totalServerErrors;
        private long _totalClientErrors;

        public long TotalRequests => Interlocked.Read(ref _totalRequests);
        public long TotalServerErrors => Interlocked.Read(ref _totalServerErrors);
        public long TotalClientErrors => Interlocked.Read(ref _totalClientErrors);

        public void ObserveRequest(string method, PathString path, int statusCode, long elapsedMs)
            => ObserveRequest(method, path.ToString(), statusCode, elapsedMs);

        public void ObserveRequest(string method, string? path, int statusCode, long elapsedMs)
        {
            Interlocked.Increment(ref _totalRequests);
            if (statusCode >= 500) Interlocked.Increment(ref _totalServerErrors);
            else if (statusCode >= 400) Interlocked.Increment(ref _totalClientErrors);
        }
    }
}

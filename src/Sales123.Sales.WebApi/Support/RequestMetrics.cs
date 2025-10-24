using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Sales123.Sales.WebApi.Support
{
    /// <summary>
    /// Métricas in-memory, sem libs externas.
    /// - Totais (requests, 4xx, 5xx)
    /// - Por rota (METHOD + path): contagem, 4xx/5xx e buckets de latência
    /// - p95/p99 aproximados por buckets
    /// Exposto via GET /metrics-app (Snapshot) e logado periodicamente pelo MetricsSummaryHostedService.
    /// </summary>
    public sealed class RequestMetrics
    {
        public const string ActivitySourceName = "Sales123.Sales.WebApi";
        public const string MeterName = "Sales123.Sales.WebApi";

        private long _totalRequests;
        private long _totalServerErrors;
        private long _totalClientErrors;

        // <10, <50, <100, <250, <500, <1000, >=1000 ms
        private static readonly int[] _edges = new[] { 10, 50, 100, 250, 500, 1000 };
        private const int BucketsCount = 7;

        private sealed class RouteAgg
        {
            public long Count;
            public long Errors4xx;
            public long Errors5xx;
            public long[] Buckets = new long[BucketsCount];
        }

        private readonly ConcurrentDictionary<string, RouteAgg> _routes = new();

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

            var key = $"{method.ToUpperInvariant()} {path ?? "unknown"}";
            var agg = _routes.GetOrAdd(key, _ => new RouteAgg());

            Interlocked.Increment(ref agg.Count);
            if (statusCode >= 500) Interlocked.Increment(ref agg.Errors5xx);
            else if (statusCode >= 400) Interlocked.Increment(ref agg.Errors4xx);

            var bucket = BucketIndex(elapsedMs);
            Interlocked.Increment(ref agg.Buckets[bucket]);
        }

        public object Snapshot()
        {
            var routes = _routes.Select(kv =>
            {
                var p95 = ApproxPercentile(kv.Value.Buckets, 0.95);
                var p99 = ApproxPercentile(kv.Value.Buckets, 0.99);
                return new
                {
                    route = kv.Key,
                    count = kv.Value.Count,
                    errors4xx = kv.Value.Errors4xx,
                    errors5xx = kv.Value.Errors5xx,
                    p95Ms = p95,
                    p99Ms = p99,
                    buckets = new Dictionary<string, long>
                    {
                        ["lt10ms"] = kv.Value.Buckets[0],
                        ["lt50ms"] = kv.Value.Buckets[1],
                        ["lt100ms"] = kv.Value.Buckets[2],
                        ["lt250ms"] = kv.Value.Buckets[3],
                        ["lt500ms"] = kv.Value.Buckets[4],
                        ["lt1s"] = kv.Value.Buckets[5],
                        ["ge1s"] = kv.Value.Buckets[6]
                    }
                };
            })
            .OrderByDescending(r => r.p95Ms)
            .ThenByDescending(r => r.count)
            .ToArray();

            return new
            {
                totals = new
                {
                    requests = Interlocked.Read(ref _totalRequests),
                    errors4xx = Interlocked.Read(ref _totalClientErrors),
                    errors5xx = Interlocked.Read(ref _totalServerErrors)
                },
                routes
            };
        }

        /// <summary>Resumo forte-mente tipado para o hosted service.</summary>
        public IReadOnlyList<RouteSummary> TopRoutesByP95(int top = 3)
        {
            return _routes.Select(kv => new RouteSummary(
                    Route: kv.Key,
                    Count: kv.Value.Count,
                    Errors4xx: kv.Value.Errors4xx,
                    Errors5xx: kv.Value.Errors5xx,
                    P95Ms: ApproxPercentile(kv.Value.Buckets, 0.95),
                    P99Ms: ApproxPercentile(kv.Value.Buckets, 0.99)))
                .OrderByDescending(r => r.P95Ms).ThenByDescending(r => r.Count)
                .Take(top).ToArray();
        }

        public (long total, long e4xx, long e5xx) Totals()
            => (TotalRequests, TotalClientErrors, TotalServerErrors);

        public void ResetWindow()
        {
            Interlocked.Exchange(ref _totalRequests, 0);
            Interlocked.Exchange(ref _totalClientErrors, 0);
            Interlocked.Exchange(ref _totalServerErrors, 0);
            _routes.Clear();
        }

        public readonly record struct RouteSummary(string Route, long Count, long Errors4xx, long Errors5xx, int P95Ms, int P99Ms);

        private static int BucketIndex(double ms)
        {
            if (ms < 10) return 0;
            if (ms < 50) return 1;
            if (ms < 100) return 2;
            if (ms < 250) return 3;
            if (ms < 500) return 4;
            if (ms < 1000) return 5;
            return 6;
        }

        private static int ApproxPercentile(long[] buckets, double p)
        {
            var total = buckets.Sum(b => b);
            if (total == 0) return 0;

            var target = (long)Math.Ceiling(total * p);
            long cum = 0;

            for (int i = 0; i < buckets.Length; i++)
            {
                cum += buckets[i];
                if (cum >= target)
                {
                    // retorna o limite superior do bucket como aproximação do percentil
                    if (i < _edges.Length) return _edges[i];
                    return 1200; // ">=1s": devolve um valor alto representativo
                }
            }
            return 1200;
        }
    }
}

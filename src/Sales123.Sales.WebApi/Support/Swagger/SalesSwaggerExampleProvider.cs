using Microsoft.OpenApi.Any;

namespace Sales123.Sales.WebApi.Support.Swagger
{
    public interface ISalesSwaggerExampleProvider
    {
        string NewCorrelationId();
        OpenApiObject BuildCreateRequestExample();
        OpenApiObject BuildSaleEnvelopeExample(string? status = null);
        OpenApiObject BuildPagedListEnvelopeExample();
        OpenApiObject BuildValidation422EnvelopeExample();
        OpenApiObject BuildNotFoundEnvelopeExample();
    }

    public sealed class SalesSwaggerExampleProvider : ISalesSwaggerExampleProvider
    {
        public string NewCorrelationId() => Guid.NewGuid().ToString("N");

        public OpenApiObject BuildCreateRequestExample()
        {
            var rnd = Random.Shared;

            var items = new OpenApiArray();
            var n = rnd.Next(1, 4);
            for (int i = 0; i < n; i++)
            {
                items.Add(new OpenApiObject
                {
                    ["productId"] = new OpenApiString(Guid.NewGuid().ToString("N")),
                    ["productName"] = new OpenApiString($"Produto {rnd.Next(1, 9999)}"),
                    ["sku"] = new OpenApiString($"SKU-{rnd.Next(1000, 9999)}"),
                    ["quantity"] = new OpenApiInteger(rnd.Next(1, 6)),
                    ["unitPrice"] = new OpenApiDouble(Math.Round(10 + rnd.NextDouble() * 490, 2))
                });
            }

            return new OpenApiObject
            {
                ["number"] = new OpenApiString($"{DateTime.UtcNow:yyyyMMdd}-{rnd.Next(1000, 9999)}"),
                ["saleDate"] = new OpenApiString(DateTimeOffset.UtcNow.AddMinutes(-rnd.Next(0, 10_080)).ToString("o")),
                ["customerId"] = new OpenApiString(Guid.NewGuid().ToString("N")),
                ["customerName"] = new OpenApiString($"Cliente {rnd.Next(1, 9999)}"),
                ["branchId"] = new OpenApiString($"BR-{rnd.Next(1, 99):D2}"),
                ["branchName"] = new OpenApiString($"Filial {rnd.Next(1, 99):D2}"),
                ["items"] = items
            };
        }

        public OpenApiObject BuildSaleEnvelopeExample(string? status = null)
        {
            var sale = BuildSale(status ?? Pick(new[] { "Created", "Approved", "Canceled", "InProgress" }));
            return new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["data"] = sale,
                ["traceId"] = new OpenApiString(NewCorrelationId()),
                ["elapsedMs"] = new OpenApiInteger(Random.Shared.Next(3, 120))
            };
        }

        public OpenApiObject BuildPagedListEnvelopeExample()
        {
            var items = new OpenApiArray();
            var count = Random.Shared.Next(1, 5);
            for (int i = 0; i < count; i++)
                items.Add(BuildSale(Pick(new[] { "Created", "Approved", "InProgress" })));

            return new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["data"] = new OpenApiObject
                {
                    ["page"] = new OpenApiInteger(1),
                    ["size"] = new OpenApiInteger(10),
                    ["total"] = new OpenApiInteger(42),
                    ["items"] = items
                },
                ["traceId"] = new OpenApiString(NewCorrelationId()),
                ["elapsedMs"] = new OpenApiInteger(Random.Shared.Next(5, 150))
            };
        }

        public OpenApiObject BuildValidation422EnvelopeExample()
        {
            return new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(false),
                ["data"] = new OpenApiNull(),
                ["traceId"] = new OpenApiString(NewCorrelationId()),
                ["elapsedMs"] = new OpenApiInteger(Random.Shared.Next(3, 120)),
                ["notifications"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["code"]     = new OpenApiString("SALE_QUANTITY_INVALID"),
                        ["message"]  = new OpenApiString("A quantidade deve ser maior que zero."),
                        ["path"]     = new OpenApiString("items[0].quantity"),
                        ["severity"] = new OpenApiInteger(3)
                    },
                    new OpenApiObject
                    {
                        ["code"]     = new OpenApiString("SALE_ITEM_DUPLICATED"),
                        ["message"]  = new OpenApiString("Produto já adicionado na venda."),
                        ["path"]     = new OpenApiString("items[0].productId"),
                        ["severity"] = new OpenApiInteger(2)
                    }
                }
            };
        }

        public OpenApiObject BuildNotFoundEnvelopeExample()
        {
            return new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(false),
                ["data"] = new OpenApiNull(),
                ["traceId"] = new OpenApiString(NewCorrelationId()),
                ["elapsedMs"] = new OpenApiInteger(Random.Shared.Next(1, 40)),
                ["notifications"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["code"]     = new OpenApiString("SALE_NOT_FOUND"),
                        ["message"]  = new OpenApiString("Venda não encontrada."),
                        ["path"]     = new OpenApiString("id"),
                        ["severity"] = new OpenApiInteger(3)
                    }
                }
            };
        }

        private static OpenApiObject BuildSale(string status)
        {
            var rnd = Random.Shared;
            var items = new OpenApiArray();
            decimal saleTotal = 0;

            var n = rnd.Next(1, 4);
            for (int i = 0; i < n; i++)
            {
                var q = rnd.Next(1, 6);
                var up = Math.Round(10 + rnd.NextDouble() * 490, 2);
                var discount = new[] { 0, 0, 0, 5, 10 }[rnd.Next(0, 5)];
                var itemTotal = Math.Round(q * up * (1 - discount / 100.0), 2);
                saleTotal += (decimal)itemTotal;

                items.Add(new OpenApiObject
                {
                    ["id"] = new OpenApiString(Guid.NewGuid().ToString("N")),
                    ["productId"] = new OpenApiString(Guid.NewGuid().ToString("N")),
                    ["productName"] = new OpenApiString($"Produto {rnd.Next(1, 9999)}"),
                    ["sku"] = new OpenApiString($"SKU-{rnd.Next(1000, 9999)}"),
                    ["quantity"] = new OpenApiInteger(q),
                    ["discountPercent"] = new OpenApiInteger(discount),
                    ["unitPrice"] = new OpenApiDouble(up),
                    ["itemTotal"] = new OpenApiDouble((double)itemTotal),
                    ["canceled"] = new OpenApiBoolean(false)
                });
            }

            return new OpenApiObject
            {
                ["id"] = new OpenApiString(Guid.NewGuid().ToString("N")),
                ["number"] = new OpenApiString($"{DateTime.UtcNow:yyyyMMdd}-{rnd.Next(1000, 9999)}"),
                ["saleDate"] = new OpenApiString(DateTimeOffset.UtcNow.ToString("o")),
                ["customerId"] = new OpenApiString(Guid.NewGuid().ToString("N")),
                ["customerName"] = new OpenApiString($"Cliente {rnd.Next(1, 9999)}"),
                ["branchId"] = new OpenApiString($"BR-{rnd.Next(1, 99):D2}"),
                ["branchName"] = new OpenApiString($"Filial {rnd.Next(1, 99):D2}"),
                ["status"] = new OpenApiString(status),
                ["saleTotal"] = new OpenApiDouble(Math.Round((double)saleTotal, 2)),
                ["items"] = items
            };
        }

        private static T Pick<T>(IReadOnlyList<T> v) => v[Random.Shared.Next(0, v.Count)];
    }
}

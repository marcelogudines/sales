
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.Domain.Validation;

namespace Sales123.Sales.Domain.ValueObjects;

public sealed record ProductRef(string Id, string Name, string? Sku)
{
    public static Result<ProductRef> Create(string? id, string? name, string? sku, string path = "product")
    {
        var notification = new NotificationsBag();
        Ensure.Required(notification, id,   $"{path}.id", $"{path}.id_required");
        Ensure.Required(notification, name, $"{path}.name", $"{path}.name_required");
        if (notification.HasErrors) return Result<ProductRef>.From(null, notification);

        var skuVal = string.IsNullOrWhiteSpace(sku) ? null : sku!.Trim();
        return Result.Ok(new ProductRef(id!.Trim(), name!.Trim(), skuVal));
    }
}

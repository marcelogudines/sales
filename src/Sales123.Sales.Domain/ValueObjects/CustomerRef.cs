
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.Domain.Validation;

namespace Sales123.Sales.Domain.ValueObjects;

public sealed record CustomerRef(string Id, string Name)
{
    public static Result<CustomerRef> Create(string? id, string? name, string path = "customer")
    {
        var notification = new NotificationsBag();
        Ensure.Required(notification, id,   $"{path}.id", $"{path}.id_required");
        Ensure.Required(notification, name, $"{path}.name", $"{path}.name_required");
        return notification.HasErrors ? Result<CustomerRef>.From(null, notification)
                             : Result.Ok(new CustomerRef(id!.Trim(), name!.Trim()));
    }
}

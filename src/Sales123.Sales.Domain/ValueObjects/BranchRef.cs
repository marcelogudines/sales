
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.Domain.Validation;

namespace Sales123.Sales.Domain.ValueObjects;

public sealed record BranchRef(string Id, string Name)
{
    public static Result<BranchRef> Create(string? id, string? name, string path = "branch")
    {
        var notification = new NotificationsBag();
        Ensure.Required(notification, id,   $"{path}.id", $"{path}.id_required");
        Ensure.Required(notification, name, $"{path}.name", $"{path}.name_required");
        return notification.HasErrors ? Result<BranchRef>.From(null, notification)
                             : Result.Ok(new BranchRef(id!.Trim(), name!.Trim()));
    }
}

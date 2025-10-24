using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Policies;
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.Domain.ValueObjects;

namespace Sales123.Sales.Domain.Entities;

public sealed class SaleItem : Notifiable
{
    private const string ErrProductRequired = "item.product_required";
    private const string ErrUnitRequired = "item.unit_price_required";
    private const string ErrQuantityRange = "item.quantity_range";
    private const string ErrCanceledMutation = "item.canceled_mutation";

    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public ProductRef Product { get; private set; } = default!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;
    public int DiscountPercent { get; private set; }
    public Money ItemTotal { get; private set; } = Money.Zero;
    public bool Canceled { get; private set; }
    private bool IsCanceled => Canceled;

    private SaleItem() { }

    public static Result<SaleItem> Create(ProductRef product, int quantity, Money unitPrice, string path = "items[]")
    {
        var notes = new NotificationsBag();

        ValidateRequired(notes, product, unitPrice, path);
        ValidateQuantity(notes, quantity, $"{path}.quantity");

        if (notes.HasErrors) return Result<SaleItem>.From(null, notes);

        var item = new SaleItem
        {
            Product = product,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
        item.Recalculate();
        return Result.Ok(item);
    }

    public Result<SaleItem> ReplaceQuantity(int newQuantity, string path = "items[].quantity")
    {
        if (IsCanceled)
            return Result.Fail<SaleItem>(new Notifications.Notification(
                ErrCanceledMutation, "Não é possível alterar item cancelado.", path));

        var notes = new NotificationsBag();
        ValidateQuantity(notes, newQuantity, path);
        if (notes.HasErrors) return Result<SaleItem>.From(null, notes);

        Quantity = newQuantity;
        Recalculate();
        return Result.Ok(this);
    }

    public void Cancel() => Canceled = true;

    private static void ValidateRequired(NotificationsBag bag, ProductRef product, Money unitPrice, string path)
    {
        if (product is null)
            bag.Add(ErrProductRequired, "Produto é obrigatório.", $"{path}.product");

        if (unitPrice is null)
            bag.Add(ErrUnitRequired, "Preço unitário é obrigatório.", $"{path}.unitPrice");
    }

    private static void ValidateQuantity(NotificationsBag bag, int qty, string path)
    {
        if (!QuantityDiscountPolicy.IsAllowed(qty))
            bag.Add(ErrQuantityRange,
                    $"Quantidade deve ser 1..{QuantityDiscountPolicy.MaxQuantityPerItem}.",
                    path);
    }

    private void Recalculate()
    {
        DiscountPercent = QuantityDiscountPolicy.DiscountPercentFor(Quantity);

        var gross = UnitPrice * Quantity;       
        var pct = DiscountPercent;
        if (pct < 0) pct = 0; if (pct > 100) pct = 100;  
        var factor = 1m - (pct / 100m);

        ItemTotal = Money.FromRaw(gross.Value * factor);
    }
}

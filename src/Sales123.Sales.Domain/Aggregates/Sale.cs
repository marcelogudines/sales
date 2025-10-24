using DomainNotification = Sales123.Sales.Domain.Notifications.Notification;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Entities;
using Sales123.Sales.Domain.Enums;
using Sales123.Sales.Domain.Events;
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.Domain.ValueObjects;

namespace Sales123.Sales.Domain.Aggregates;

public sealed class Sale : Notifiable
{
   
    private const string ErrCanceledMutation = "sale.canceled_mutation";
    private const string ErrItemNotFound = "sale.item_not_found";
    private const string ErrNumberRequired = "sale.number_required";
    private const string ErrCustomerRequired = "sale.customer_required";
    private const string ErrBranchRequired = "sale.branch_required";
    private const string ErrItemsMin1 = "sale.items_min_1";

  
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public string Number { get; private set; } = string.Empty;
    public DateTimeOffset SaleDate { get; private set; }
    public CustomerRef Customer { get; private set; } = default!;
    public BranchRef Branch { get; private set; } = default!;
    public SaleStatus Status { get; private set; } = SaleStatus.NotCanceled;

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Money SaleTotal => _items.Where(i => !i.Canceled)
                                    .Select(i => i.ItemTotal)
                                    .Aggregate(Money.Zero, (acc, it) => acc + it);

 
    private readonly List<IDomainEvent> _events = new();
    public IEnumerable<IDomainEvent> DequeueEvents()
    {
        var s = _events.ToArray();
        _events.Clear();
        return s;
    }
    private void Raise(IDomainEvent e) => _events.Add(e);
    private void TouchUpdated() => Raise(new SaleUpdated(Id, Number));

    private Sale() { }

    public static Result<Sale> Create(
        string? number,
        DateTimeOffset saleDate,
        CustomerRef customer,
        BranchRef branch,
        IEnumerable<SaleItemInput> itemInputs)
    {
        var notes = new NotificationsBag();

        ValidateHeader(notes, number, customer, branch);
        var items = ValidateAndBuildItems(notes, itemInputs);

        if (notes.HasErrors) return Result<Sale>.From(null, notes);

        var sale = new Sale
        {
            Number = number!.Trim(),
            SaleDate = saleDate,
            Customer = customer!,
            Branch = branch!
        };
        sale._items.AddRange(items);

        sale.Raise(new SaleCreated(sale.Id, sale.Number));
        return Result.Ok(sale);
    }

    private static void ValidateHeader(NotificationsBag notes, string? number, CustomerRef customer, BranchRef branch)
    {
        if (string.IsNullOrWhiteSpace(number))
            notes.Add(ErrNumberRequired, "Número da venda é obrigatório.", "number");
        if (customer is null)
            notes.Add(ErrCustomerRequired, "Cliente é obrigatório.", "customer");
        if (branch is null)
            notes.Add(ErrBranchRequired, "Filial é obrigatória.", "branch");
    }

    private static List<SaleItem> ValidateAndBuildItems(NotificationsBag notes, IEnumerable<SaleItemInput> itemInputs)
    {
        var inputs = itemInputs?.ToList() ?? new();
        if (inputs.Count == 0)
        {
            notes.Add(ErrItemsMin1, "Venda deve ter ao menos um item.", "items");
            return new List<SaleItem>();
        }

        var items = new List<SaleItem>(inputs.Count);
        for (var i = 0; i < inputs.Count; i++)
        {
            var e = inputs[i];
            var r = SaleItem.Create(e.Product, e.Quantity, e.UnitPrice, $"items[{i}]");
            if (!r.IsValid) notes.Merge(r.Notifications);
            else items.Add(r.Value!);
        }
        return items;
    }

    private bool IsCanceled => Status == SaleStatus.Canceled;

    private static Result<T> Fail<T>(string code, string message, string? path = null)
        => Result.Fail<T>(new DomainNotification(code, message, path));

    private Result<T>? GuardNotCanceled<T>()
        => IsCanceled ? Fail<T>(ErrCanceledMutation, "Não é possível alterar venda cancelada.") : null;

    private Result<SaleItem> FindItemOrFail(string itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        return item is null
            ? Fail<SaleItem>(ErrItemNotFound, "Item não encontrado.", "items")
            : Result.Ok(item);
    }

    public Result<SaleItem> AddItem(SaleItemInput input)
    {
        if (GuardNotCanceled<SaleItem>() is { } fail) return fail;

        var r = SaleItem.Create(input.Product, input.Quantity, input.UnitPrice, $"items[{_items.Count}]");
        if (!r.IsValid) return r;

        _items.Add(r.Value!);
        TouchUpdated();
        return Result.Ok(r.Value!);
    }

    public Result<SaleItem> ReplaceItemQuantity(string itemId, int newQuantity)
    {
        if (GuardNotCanceled<SaleItem>() is { } fail) return fail;

        var itemRes = FindItemOrFail(itemId);
        if (!itemRes.IsValid) return itemRes;

        var item = itemRes.Value!;
        var r = item.ReplaceQuantity(newQuantity, $"items[{_items.IndexOf(item)}].quantity");
        if (!r.IsValid) return r;

        TouchUpdated();
        return Result.Ok(item);
    }

    public Result<SaleItem> CancelItem(string itemId)
    {
        if (GuardNotCanceled<SaleItem>() is { } fail) return fail;

        var itemRes = FindItemOrFail(itemId);
        if (!itemRes.IsValid) return itemRes;

        var item = itemRes.Value!;
        if (!item.Canceled)
        {
            item.Cancel();
            Raise(new SaleItemCanceled(Id, Number, item.Id));
            TouchUpdated();
        }
        return Result.Ok(item);
    }

    public Result<Sale> Cancel(string? reason = null)
    {
        if (IsCanceled) return Result.Ok(this);

        Status = SaleStatus.Canceled;
        Raise(new SaleCanceled(Id, Number, reason));
        return Result.Ok(this);
    }
}

using Sales123.Sales.Application.Abstractions;
using Sales123.Sales.Application.DTOs;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Aggregates;
using Sales123.Sales.Domain.Entities;
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.Domain.ValueObjects;
using DomainNotification = Sales123.Sales.Domain.Notifications.Notification;

namespace Sales123.Sales.Application.Services;

public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _repo;
    private readonly ISaleQueryRepository _query;
    private readonly IDomainEventPublisher _publisher;

    public SaleService(ISaleRepository repo, ISaleQueryRepository query, IDomainEventPublisher publisher)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _query = query ?? throw new ArgumentNullException(nameof(query));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public Result<Sale> Create(CreateSaleCommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var notifications = new NotificationsBag();

        var customer = CustomerRef.Create(command.CustomerId, command.CustomerName, "customer");
        var branch = BranchRef.Create(command.BranchId, command.BranchName, "branch");

        if (!customer.IsValid) notifications.Merge(customer.Notifications);
        if (!branch.IsValid) notifications.Merge(branch.Notifications);

        // Monta itens + coleta notificações (DRY)
        var itemsRes = BuildItemInputs(command.Items, notifications);
        if (notifications.HasErrors) // erros de customer/branch/itens
            return Result<Sale>.From(null, notifications);

        var saleRes = Sale.Create(command.Number, command.SaleDate, customer.Value!, branch.Value!, itemsRes);
        if (!saleRes.IsValid) return saleRes;

        var (created, persisted) = _repo.AddIfNotExists(saleRes.Value!);

        if (created)
        {
            PublishRaised(saleRes.Value!);
            return Result<Sale>.From(persisted, new NotificationsBag());
        }

        var nb = new NotificationsBag();
        nb.Add(new DomainNotification(
            "sale.already_exists",
            "Venda já existe para este número e filial.",
            "number"
        ));
        return Result<Sale>.From(persisted, nb);
    }

    public Result<SaleItem> AddItem(AddItemCommand command)
    {
        if (command is null)
            return Fail<SaleItem>("sale.command_required", "Comando é obrigatório.", "command");

        var sale = _query.GetById(command.SaleId);
        if (sale is null) return NotFound<SaleItem>("sale", "saleId");

        var product = ProductRef.Create(command.ProductId, command.ProductName, command.Sku, "item.product");
        var unit = Money.From(command.UnitPrice, "item.unitPrice");

        var notifications = new NotificationsBag();
        if (!product.IsValid) notifications.Merge(product.Notifications);
        if (!unit.IsValid) notifications.Merge(unit.Notifications);
        if (notifications.HasErrors) return Result<SaleItem>.From(null, notifications);

        var res = sale.AddItem(new SaleItemInput(product.Value!, command.Quantity, unit.Value!));
        return PersistAndPublish(sale, res);
    }

    public Result<SaleItem> ReplaceItemQuantity(ReplaceItemQuantityCommand command)
    {
        if (command is null)
            return Fail<SaleItem>("sale.command_required", "Comando é obrigatório.", "command");

        var sale = _query.GetById(command.SaleId);
        if (sale is null) return NotFound<SaleItem>("sale", "saleId");

        var res = sale.ReplaceItemQuantity(command.ItemId, command.NewQuantity);
        return PersistAndPublish(sale, res);
    }

    public Result<SaleItem> CancelItem(CancelItemCommand command)
    {
        if (command is null)
            return Fail<SaleItem>("sale.command_required", "Comando é obrigatório.", "command");

        var sale = _query.GetById(command.SaleId);
        if (sale is null) return NotFound<SaleItem>("sale", "saleId");

        var res = sale.CancelItem(command.ItemId);
        return PersistAndPublish(sale, res);
    }

    public Result<Sale> CancelSale(CancelSaleCommand command)
    {
        if (command is null)
            return Fail<Sale>("sale.command_required", "Comando é obrigatório.", "command");

        var sale = _query.GetById(command.SaleId);
        if (sale is null) return NotFound<Sale>("sale", "saleId");

        var res = sale.Cancel(command.Reason);
        return PersistAndPublish(sale, res);
    }

    public bool Delete(string saleId) => _repo.Delete(saleId);

    private static Result<T> Fail<T>(string code, string message, string path) =>
        Result.Fail<T>(new DomainNotification(code, message, path));

    private static Result<T> NotFound<T>(string entity, string fieldPath) =>
        Result.Fail<T>(new DomainNotification($"{entity}.not_found", $"{entity.First().ToString().ToUpper() + entity[1..]} não encontrada.", fieldPath));

    private List<SaleItemInput> BuildItemInputs(IEnumerable<CreateSaleItemDto>? items, NotificationsBag bag)
    {
        var result = new List<SaleItemInput>();
        if (items is null) return result;

        var index = 0;
        foreach (var dto in items)
        {
            var product = ProductRef.Create(dto.ProductId, dto.ProductName, dto.Sku, $"items[{index}].product");
            var unit = Money.From(dto.UnitPrice, $"items[{index}].unitPrice");

            if (!product.IsValid) bag.Merge(product.Notifications);
            if (!unit.IsValid) bag.Merge(unit.Notifications);

            if (product.IsValid && unit.IsValid)
                result.Add(new SaleItemInput(product.Value!, dto.Quantity, unit.Value!));

            index++;
        }

        return result;
    }

    private Result<T> PersistAndPublish<T>(Sale sale, Result<T> res)
    {
        if (!res.IsValid) return res;
        _repo.Update(sale);
        PublishRaised(sale);
        return res;
    }

    private void PublishRaised(Sale sale)
    {
        var evts = sale.DequeueEvents();
        if (evts is null) return;
        _publisher.Publish(evts);
    }
}

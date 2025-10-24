using Sales123.Sales.Application.DTOs;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Aggregates;
using Sales123.Sales.Domain.Entities;

namespace Sales123.Sales.Application.Services;

public interface ISaleService
{
    Result<Sale> Create(CreateSaleCommand command);
    Result<SaleItem> AddItem(AddItemCommand command);
    Result<SaleItem> ReplaceItemQuantity(ReplaceItemQuantityCommand command);
    Result<SaleItem> CancelItem(CancelItemCommand command);
    Result<Sale> CancelSale(CancelSaleCommand command);
    bool Delete(string saleId);
}

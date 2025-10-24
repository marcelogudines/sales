
using Sales123.Sales.Domain.Aggregates;

namespace Sales123.Sales.Application.Abstractions;

public interface ISaleQueryRepository
{
    Sale? GetById(string id);
    (IEnumerable<Sale> Items, long Total) List(int page, int pageSize);
    Sale? GetByNumberAndBranch(string number, string branchId);
}
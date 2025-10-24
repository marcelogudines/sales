using Sales123.Sales.Application.Abstractions;
using Sales123.Sales.Domain.Aggregates;
using System.Collections.Concurrent;

namespace Sales123.Sales.Infra.Persistence.InMemory;

public sealed class InMemorySaleRepository : ISaleRepository, ISaleQueryRepository
{
    
    private readonly ConcurrentDictionary<string, Sale> _byId = new();
    private readonly ConcurrentDictionary<(string Number, string BranchId), Sale> _byKey = new();

    public void Add(Sale sale)
    {
      
        _byId[sale.Id] = sale;
        _byKey[(sale.Number, sale.Branch.Id)] = sale;
    }

    public (bool created, Sale sale) AddIfNotExists(Sale sale)
    {
        var key = (sale.Number, sale.Branch.Id);
        if (_byKey.TryGetValue(key, out var existing))
            return (false, existing);

        _byId[sale.Id] = sale;
        _byKey[key] = sale;
        return (true, sale);
    }

    public Sale? GetById(string id) => _byId.TryGetValue(id, out var sale) ? sale : null;

    public Sale? GetByNumberAndBranch(string number, string branchId)
        => _byKey.TryGetValue((number, branchId), out var sale) ? sale : null;

    public void Update(Sale sale)
    {
        if (_byId.TryGetValue(sale.Id, out var previous))
        {
            var prevKey = (previous.Number, previous.Branch.Id);
            var newKey = (sale.Number, sale.Branch.Id);
            if (!prevKey.Equals(newKey))
            {
                _byKey.TryRemove(prevKey, out _);
                _byKey[newKey] = sale;
            }
        }

        _byId[sale.Id] = sale;
        _byKey[(sale.Number, sale.Branch.Id)] = sale;
    }

    public bool Delete(string id)
    {
        if (_byId.TryRemove(id, out var removed))
        {
            _byKey.TryRemove((removed.Number, removed.Branch.Id), out _);
            return true;
        }
        return false;
    }

    public (IEnumerable<Sale> Items, long Total) List(int page, int pageSize)
    {
        var all = _byId.Values.ToList();
        var total = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (items, total);
    }
}

namespace Sales123.Sales.Application.Abstractions
{
    using Sales123.Sales.Domain.Aggregates;
    public interface ISaleRepository
    {
        void Add(Sale sale);
        void Update(Sale sale);
        bool Delete(string id);
        (bool created, Sale sale) AddIfNotExists(Sale sale);
    }

}


using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Shared;

namespace Sales123.Sales.Domain.ValueObjects;

public sealed record Money
{
    public decimal Value { get; }
    private Money(decimal value) => Value = decimal.Round(value, 2, System.MidpointRounding.AwayFromZero);

    public static Result<Money> From(decimal? value, string path = "value")
    {
        var notification = new NotificationsBag();
        if (value is null) notification.Add("money.required", "Valor é obrigatório.", path);
        else if (value < 0) notification.Add("money.non_negative", "Valor deve ser >= 0.", path);

        return notification.HasErrors ? Result<Money>.From(null, notification) : Result.Ok(new Money(value!.Value));
    }

    public static Money FromRaw(decimal value) => new(value);

    public static Money Zero => new(0m);
    public static Money operator +(Money a, Money b) => new(a.Value + b.Value);
    public static Money operator *(Money a, int m) => new(a.Value * m);
    public static Money operator *(Money a, decimal d) => new(a.Value * d);

    public override string ToString() => Value.ToString("0.00");
}

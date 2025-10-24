
namespace Sales123.Sales.Domain.Policies;

public static class QuantityDiscountPolicy
{
    public const int MaxQuantityPerItem = 20;

    public static bool IsAllowed(int qty) => qty >= 1 && qty <= MaxQuantityPerItem;

    public static int DiscountPercentFor(int qty)
    {
        if (qty >= 10) return 20;
        if (qty >= 5)  return 10;
        return 0;
    }
}

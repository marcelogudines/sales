
using Sales123.Sales.Domain.Shared;

namespace Sales123.Sales.Domain.Validation;

public static class Ensure
{
    public static void Required(NotificationsBag notification, string? value, string path, string code, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            notification.Add(code, message ?? $"{path} é obrigatório.", path);
    }

    public static void Range(NotificationsBag notification, int value, int min, int max, string path, string code, string? message = null)
    {
        if (value < min || value > max)
            notification.Add(code, message ?? $"{path} deve estar entre {min} e {max}.", path);
    }

    public static void NonNegative(NotificationsBag notification, decimal value, string path, string code, string? message = null)
    {
        if (value < 0)
            notification.Add(code, message ?? $"{path} deve ser >= 0.", path);
    }
}

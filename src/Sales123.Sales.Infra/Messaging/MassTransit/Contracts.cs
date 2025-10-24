
namespace Sales123.Sales.Infra.Messaging.MassTransit;

public sealed record SaleCreatedMessage(string SaleId, string Number, DateTimeOffset OccurredAt);
public sealed record SaleUpdatedMessage(string SaleId, string Number, DateTimeOffset OccurredAt);
public sealed record SaleCanceledMessage(string SaleId, string Number, string? Reason, DateTimeOffset OccurredAt);
public sealed record SaleItemCanceledMessage(string SaleId, string Number, string ItemId, DateTimeOffset OccurredAt);
public sealed record ApiErrorOccurred(
    string TraceId,
    string? CorrelationId,
    string Method,
    string Url,             
    string Path,             
    int StatusCode,
    string Message,
    string? ExceptionType,
    string? StackTrace,      
    string? RequestContentType,
    string? RequestBody,    
    string? SourceType,     
    string? SourceMethod,    
    string? SourceFile,      
    int? SourceLine,      
    string? FriendlyStack,  
    DateTimeOffset OccurredAt
);
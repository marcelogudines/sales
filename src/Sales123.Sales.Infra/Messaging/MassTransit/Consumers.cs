
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Sales123.Sales.Infra.Messaging.MassTransit;

public sealed class SaleCreatedConsumer : IConsumer<SaleCreatedMessage>
{
    private readonly ILogger<SaleCreatedConsumer> _logger;
    public SaleCreatedConsumer(ILogger<SaleCreatedConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<SaleCreatedMessage> context)
    {
        _logger.LogInformation("Evento recebido: SaleCreated {SaleId} {Number} @ {At}", context.Message.SaleId, context.Message.Number, context.Message.OccurredAt);
        return Task.CompletedTask;
    }
}

public sealed class SaleUpdatedConsumer : IConsumer<SaleUpdatedMessage>
{
    private readonly ILogger<SaleUpdatedConsumer> _logger;
    public SaleUpdatedConsumer(ILogger<SaleUpdatedConsumer> logger) => _logger = logger;
    public Task Consume(ConsumeContext<SaleUpdatedMessage> context)
    {
        _logger.LogInformation("Evento recebido: SaleUpdated {SaleId} {Number} @ {At}", context.Message.SaleId, context.Message.Number, context.Message.OccurredAt);
        return Task.CompletedTask;
    }
}

public sealed class SaleCanceledConsumer : IConsumer<SaleCanceledMessage>
{
    private readonly ILogger<SaleCanceledConsumer> _logger;
    public SaleCanceledConsumer(ILogger<SaleCanceledConsumer> logger) => _logger = logger;
    public Task Consume(ConsumeContext<SaleCanceledMessage> context)
    {
        _logger.LogInformation("Evento recebido: SaleCanceled {SaleId} {Number} reason={Reason} @ {At}", context.Message.SaleId, context.Message.Number, context.Message.Reason, context.Message.OccurredAt);
        return Task.CompletedTask;
    }
}

public sealed class SaleItemCanceledConsumer : IConsumer<SaleItemCanceledMessage>
{
    private readonly ILogger<SaleItemCanceledConsumer> _logger;
    public SaleItemCanceledConsumer(ILogger<SaleItemCanceledConsumer> logger) => _logger = logger;
    public Task Consume(ConsumeContext<SaleItemCanceledMessage> context)
    {
        _logger.LogInformation("Evento recebido: SaleItemCanceled {SaleId} {Number} item={ItemId} @ {At}", context.Message.SaleId, context.Message.Number, context.Message.ItemId, context.Message.OccurredAt);
        return Task.CompletedTask;
    }
}

public sealed class ApiErrorOccurredConsumer : IConsumer<ApiErrorOccurred>
{
    private readonly ILogger<ApiErrorOccurredConsumer> _logger;
    public ApiErrorOccurredConsumer(ILogger<ApiErrorOccurredConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<ApiErrorOccurred> ctx)
    {
        var e = ctx.Message;

        _logger.LogError(
            "ApiErrorOccurred: traceId={TraceId}, correlationId={CorrelationId}, {Method} {Url}, status={Status}, type={Type}, source={Source}@{File}:{Line}, message={Message}, ctype={ContentType}, req={Request}\n{FriendlyStack}",
            e.TraceId,
            e.CorrelationId,
            e.Method,
            e.Url,
            e.StatusCode,
            e.ExceptionType,
            (e.SourceType is null || e.SourceMethod is null) ? "<n/a>" : $"{e.SourceType}.{e.SourceMethod}",
            e.SourceFile ?? "<n/a>",
            e.SourceLine ?? 0,
            e.Message,
            e.RequestContentType ?? "<none>",
            string.IsNullOrEmpty(e.RequestBody) ? "<empty>" : e.RequestBody,
            e.FriendlyStack ?? "<no-stack>"
        );

        return Task.CompletedTask;
    }
}


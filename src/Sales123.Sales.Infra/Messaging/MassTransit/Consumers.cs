using MassTransit;
using Microsoft.Extensions.Logging;

namespace Sales123.Sales.Infra.Messaging.MassTransit;

internal static class ConsumerLogHelpers
{
    public static string? Correlation(ConsumeContext ctx)
    {
        if (ctx.Headers.TryGetHeader("x-correlation-id", out var h) && h is not null)
            return h.ToString();

        if (ctx.CorrelationId.HasValue)
            return ctx.CorrelationId.Value.ToString("N");

        return null;
    }

    public static string? MessageId(ConsumeContext ctx) =>
        ctx.MessageId?.ToString("N");

    public static string? ConversationId(ConsumeContext ctx) =>
        ctx.ConversationId?.ToString("N");
}

public sealed class SaleCreatedConsumer : IConsumer<SaleCreatedMessage>
{
    private readonly ILogger<SaleCreatedConsumer> _logger;
    public SaleCreatedConsumer(ILogger<SaleCreatedConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<SaleCreatedMessage> ctx)
    {
        var m = ctx.Message;
        _logger.LogInformation(
            "DomainEvent SaleCreated saleId={SaleId} number={Number} at={At} correlationId={CorrelationId} messageId={MessageId} conversationId={ConversationId}",
            m.SaleId, m.Number, m.OccurredAt,
            ConsumerLogHelpers.Correlation(ctx),
            ConsumerLogHelpers.MessageId(ctx),
            ConsumerLogHelpers.ConversationId(ctx));
        return Task.CompletedTask;
    }
}

public sealed class SaleUpdatedConsumer : IConsumer<SaleUpdatedMessage>
{
    private readonly ILogger<SaleUpdatedConsumer> _logger;
    public SaleUpdatedConsumer(ILogger<SaleUpdatedConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<SaleUpdatedMessage> ctx)
    {
        var m = ctx.Message;
        _logger.LogInformation(
            "DomainEvent SaleUpdated saleId={SaleId} number={Number} at={At} correlationId={CorrelationId} messageId={MessageId} conversationId={ConversationId}",
            m.SaleId, m.Number, m.OccurredAt,
            ConsumerLogHelpers.Correlation(ctx),
            ConsumerLogHelpers.MessageId(ctx),
            ConsumerLogHelpers.ConversationId(ctx));
        return Task.CompletedTask;
    }
}

public sealed class SaleCanceledConsumer : IConsumer<SaleCanceledMessage>
{
    private readonly ILogger<SaleCanceledConsumer> _logger;
    public SaleCanceledConsumer(ILogger<SaleCanceledConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<SaleCanceledMessage> ctx)
    {
        var m = ctx.Message;
        _logger.LogInformation(
            "DomainEvent SaleCanceled saleId={SaleId} number={Number} reason={Reason} at={At} correlationId={CorrelationId} messageId={MessageId} conversationId={ConversationId}",
            m.SaleId, m.Number, m.Reason, m.OccurredAt,
            ConsumerLogHelpers.Correlation(ctx),
            ConsumerLogHelpers.MessageId(ctx),
            ConsumerLogHelpers.ConversationId(ctx));
        return Task.CompletedTask;
    }
}

public sealed class SaleItemCanceledConsumer : IConsumer<SaleItemCanceledMessage>
{
    private readonly ILogger<SaleItemCanceledConsumer> _logger;
    public SaleItemCanceledConsumer(ILogger<SaleItemCanceledConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<SaleItemCanceledMessage> ctx)
    {
        var m = ctx.Message;
        _logger.LogInformation(
            "DomainEvent SaleItemCanceled saleId={SaleId} number={Number} itemId={ItemId} at={At} correlationId={CorrelationId} messageId={MessageId} conversationId={ConversationId}",
            m.SaleId, m.Number, m.ItemId, m.OccurredAt,
            ConsumerLogHelpers.Correlation(ctx),
            ConsumerLogHelpers.MessageId(ctx),
            ConsumerLogHelpers.ConversationId(ctx));
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
            "ApiErrorOccurred: traceId={TraceId}, correlationId={CorrelationId}, {Method} {Url}, status={Status}, type={Type}, source={Source}@{File}:{Line}, message={Message}, ctype={ContentType}, req={Request}\n{FriendlyStack} | messageId={MessageId} conversationId={ConversationId}",
            e.TraceId,
            e.CorrelationId ?? ConsumerLogHelpers.Correlation(ctx),
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
            e.FriendlyStack ?? "<no-stack>",
            ConsumerLogHelpers.MessageId(ctx),
            ConsumerLogHelpers.ConversationId(ctx)
        );

        return Task.CompletedTask;
    }
}

using MassTransit;

namespace MassTransitRabbitMq.Filters;

public class ExceptionAuditFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<ExceptionAuditFilter<T>> _logger;

    public ExceptionAuditFilter(ILogger<ExceptionAuditFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception)
        {
            _logger.LogInformation("Exception was caught and rethrown by ExceptionAuditFilter");
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("exception-audit");
    }
}
using MassTransit;

namespace MassTransitRabbitMq.Filters;

public class ExceptionAuditConsumerFilter<THandler> : IFilter<ConsumerConsumeContext<THandler>> where THandler : class 
{
    public async Task Send(ConsumerConsumeContext<THandler> context, IPipe<ConsumerConsumeContext<THandler>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception)
        {
            Console.WriteLine("Exception was caught and rethrown by ExceptionAuditConsumerFilter");
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("audit-consumer-filter");
    }
}
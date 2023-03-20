using MassTransit;

namespace MassTransitRabbitMq.Filters;

public class HandlerTypeContextInitializerFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        context.AddOrUpdatePayload(() => new HandlerContextPayload(), existing => existing);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("handler-type-context-initializer");
    }
}
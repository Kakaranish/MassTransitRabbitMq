using MassTransit;

namespace MassTransitRabbitMq.Filters;

public class HandlerTypeForwarderFilter<THandler> : IFilter<ConsumerConsumeContext<THandler>> where THandler : class 
{
    public async Task Send(ConsumerConsumeContext<THandler> context, IPipe<ConsumerConsumeContext<THandler>> next)
    {
        try
        {
            await next.Send(context);
        }
        finally
        {
            var handlerContext = context.GetPayload<HandlerContextPayload>();
            handlerContext.SetHandlerType<THandler>();
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("audit-consumer-filter");
    }
}
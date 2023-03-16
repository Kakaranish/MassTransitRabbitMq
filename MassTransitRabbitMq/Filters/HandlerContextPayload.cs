namespace MassTransitRabbitMq.Filters;

public class HandlerContextPayload
{
    private Type? _handlerType;

    public void SetHandlerType<THandler>()
    {
        _handlerType = typeof(THandler);
    }

    public Type GetHandlerType()
    {
        return _handlerType ?? throw new InvalidOperationException("Handler type is not set");
    }
}
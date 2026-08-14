namespace SIGRA.Services.Handlers;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyList<object> events);
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public async Task DispatchAsync(IReadOnlyList<object> events)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync")!;
                await (Task)method.Invoke(handler, new[] { domainEvent })!;
            }
        }
    }
}

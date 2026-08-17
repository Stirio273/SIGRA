namespace SIGRA.Services.Handlers;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyList<object> events);
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IReadOnlyList<object> events)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                try
                {
                    var method = handlerType.GetMethod("HandleAsync")!;
                    await (Task)method.Invoke(handler, new[] { domainEvent })!;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex,
                        "Erreur dans le handler {HandlerType} pour l'événement {EventType}",
                        handler.GetType().Name, domainEvent.GetType().Name);
                }
            }
        }
    }
}

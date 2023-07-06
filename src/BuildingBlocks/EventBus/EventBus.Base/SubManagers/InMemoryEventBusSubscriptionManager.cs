using EventBus.Base.Abstraction;
using EventBus.Base.Events;

namespace EventBus.Base.SubManagers;

public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
{
    private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
    private readonly List<Type> _eventTypes;

    public event EventHandler<string> OnEventRemoved;
    public Func<string, string> eventNameGetter;

    public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter)
    {
        _handlers = new Dictionary<string, List<SubscriptionInfo>>();
        _eventTypes = new List<Type>();
        this.eventNameGetter = eventNameGetter;
    }

    public bool IsEmpty => !_handlers.Any();
    
    public void Clear() => _handlers.Clear();

    public void AddSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = GetEventKey<T>();

        AddSubscription(typeof(TH), eventName);

        if (!_eventTypes.Contains(typeof(T)))
        {
            _eventTypes.Add(typeof(T));
        }
    }

    public string GetEventKey<T>()
    {
        var eventName = typeof(T).Name;

        return eventNameGetter(eventName);
    }

    public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(x => x.Name == eventName);

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
    {
        var key = GetEventKey<T>();

        return GetHandlersForEvent(key);
    }

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

    public bool HasSubscriptionForEvent<T>() where T : IntegrationEvent
    {
        var key = GetEventKey<T>();

        return HasSubscriptionForEvent(key);
    }

    public bool HasSubscriptionForEvent(string eventName) => _handlers.ContainsKey(eventName);

    public void RemoveSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var handlerToRemove = FindSubscriptionToRemove<T, TH>();
        var eventName = GetEventKey<T>();
        RemoveHandler(eventName, handlerToRemove);
    }

    #region Private Methods
    private void AddSubscription(Type handlerTye, string eventName)
    {
        if (!HasSubscriptionForEvent(eventName))
        {
            _handlers.Add(eventName, new List<SubscriptionInfo>());
        }

        if (_handlers[eventName].Any(x => x.HandlerType == handlerTye))
        {
            throw new ArgumentException($"Handler Type {handlerTye.Name} already registered for '{eventName}'", nameof(handlerTye));
        }

        _handlers[eventName].Add(SubscriptionInfo.Typed(handlerTye));
    }

    private void RemoveHandler(string eventName, SubscriptionInfo subsToRemove)
    {
        if (subsToRemove is not null)
        {
            _handlers[eventName].Remove(subsToRemove);

            if (!_handlers[eventName].Any())
            {
                _handlers.Remove(eventName);
                var eventType = _eventTypes.SingleOrDefault(x => x.Name == eventName);
                if (eventType is not null)
                {
                    _eventTypes.Remove(eventType);
                }

                RaiseOnEventRemove(eventName);
            }
        }
    }

    private void RaiseOnEventRemove(string eventName)
    {
        var handler = OnEventRemoved;
        handler?.Invoke(this, eventName);
    }

    private SubscriptionInfo FindSubscriptionToRemove<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = GetEventKey<T>();

        return FindSubscriptionToRemove(eventName, typeof(TH));
    }

    private SubscriptionInfo FindSubscriptionToRemove(string eventName, Type handlerType)
    {
        if (!HasSubscriptionForEvent(eventName))
        {
            return null;
        }

        return _handlers[eventName].SingleOrDefault(x => x.HandlerType == handlerType);
    }
    #endregion

}

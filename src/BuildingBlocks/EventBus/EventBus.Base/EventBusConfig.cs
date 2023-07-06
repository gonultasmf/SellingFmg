namespace EventBus.Base;

public class EventBusConfig
{
    public int ConnectionRetyCount { get; set; } = 5; // bir kere bağlanınca hata alınca kapanma problemi olma duruma karşı birden fazla kere denemek için
    public string DefaultTopicName { get; set; } = "SellingFmgEventBus";
    public string EventBusConnectionString { get; set; } = string.Empty;
    public string SubscriberClientAppName { get; set; } = string.Empty;
    public string EventNamePrefix { get; set; } = string.Empty;
    public string EventNameSuffix { get; set; } = "IntegrationEvent";
    public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ;
    public object Connection { get; set; } // object olmasının nedeni birden fazla message queue kullanıyor olmamızdır.

    public bool DeleteEventPrefix => !string.IsNullOrEmpty(EventNamePrefix);
    public bool DeleteEventSuffix => !string.IsNullOrEmpty(EventNameSuffix);
}

public enum EventBusType
{
    RabbitMQ,
    AzureServiceBus
}
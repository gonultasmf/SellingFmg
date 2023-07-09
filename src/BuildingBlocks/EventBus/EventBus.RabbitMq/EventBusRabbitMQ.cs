using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;

namespace EventBus.RabbitMq;

public class EventBusRabbitMQ : BaseEventBus
{
    private readonly IConnectionFactory connectionFactory;
    private readonly IModel consumerChannel;
    RabbitMQPersistentConnection persistentConnection;

    public EventBusRabbitMQ(IServiceProvider serviceProvider, EventBusConfig eventBusConfig) : base(serviceProvider, eventBusConfig)
    {
        if (eventBusConfig.Connection != null)
        {
            var connJson = JsonConvert.SerializeObject(EventBusConfig, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore // aynı isimden birden fazla iç içe yapı olunca hata vermesin diye eklendi
            });

            connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);
        }
        else
        {
            connectionFactory = new ConnectionFactory();
        }

        persistentConnection = new RabbitMQPersistentConnection(connectionFactory, eventBusConfig.ConnectionRetyCount);

        consumerChannel = CreateConsumerChannel();

        SubsManager.OnEventRemoved += SubsManager_OnEventRemoved;
    }

    public override void Publish(IntegrationEvent @event)
    {
        if (!persistentConnection.IsConnected)
        {
            persistentConnection.TryConnect();
        }

        var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(EventBusConfig.ConnectionRetyCount, x => TimeSpan.FromSeconds(Math.Pow(2, x)), (ex, time) =>
                {
                    // loglama yapılabilir.
                });

        var eventName = @event.GetType().Name;
        eventName = ProcessEventName(eventName);

        consumerChannel.ExchangeDeclare(EventBusConfig.DefaultTopicName, "direct");

        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        policy.Execute(() =>
        {
            var properties = consumerChannel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            consumerChannel.QueueDeclare(GetSubName(eventName), true, false, false, null); // create edilmediyse create eder

            consumerChannel.QueueBind(GetSubName(eventName), EventBusConfig.DefaultTopicName, eventName);

            consumerChannel.BasicPublish(EventBusConfig.DefaultTopicName, eventName, true, properties, body);
        });
    }

    public override void Subscribe<T, TH>()
    {
        var eventName = typeof(T).Name;
        eventName = ProcessEventName(eventName);

        if (!SubsManager.HasSubscriptionForEvent(eventName))
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            consumerChannel.QueueDeclare(GetSubName(eventName), true, false, false, null);

            consumerChannel.QueueBind(GetSubName(eventName), EventBusConfig.DefaultTopicName, eventName);
        }

        SubsManager.AddSubscription<T, TH>();
        StartBasicConsume(eventName);
    }

    public override void UnSubscribe<T, TH>()
    {
        SubsManager.RemoveSubscription<T, TH>();
    }

    #region Private Methods
    private IModel CreateConsumerChannel()
    {
        if (!persistentConnection.IsConnected)
        {
            persistentConnection.TryConnect();
        }

        var channel = persistentConnection.CreateModel();
        channel.ExchangeDeclare(EventBusConfig.DefaultTopicName, "direct");

        return channel;
    }

    private void StartBasicConsume(string eventName)
    {
        if (consumerChannel is not null)
        {
            var consumer = new EventingBasicConsumer(consumerChannel);

            consumer.Received += Consumer_Received;

            consumerChannel.BasicConsume(GetSubName(eventName), false, consumer);
        }
    }

    private async void Consumer_Received(object? sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        eventName = ProcessEventName(eventName);
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            // loglama yapılabilir.
        }

        consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
    }

    private void SubsManager_OnEventRemoved(object? sender, string eventName)
    {
        eventName = ProcessEventName(eventName);

        if (!persistentConnection.IsConnected)
        {
            persistentConnection.TryConnect();
        }

        consumerChannel.QueueBind(eventName, EventBusConfig.DefaultTopicName, eventName);

        if (SubsManager.IsEmpty)
        {
            consumerChannel.Close();
        }
    }
    #endregion

}

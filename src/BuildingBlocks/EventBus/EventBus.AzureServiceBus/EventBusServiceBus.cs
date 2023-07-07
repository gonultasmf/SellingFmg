using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Text;

namespace EventBus.AzureServiceBus;

public class EventBusServiceBus : BaseEventBus
{
    private ITopicClient topicClient;
    private ManagementClient managementClient;
    private ILogger logger;

    public EventBusServiceBus(IServiceProvider serviceProvider, EventBusConfig eventBusConfig) : base(serviceProvider, eventBusConfig)
    {
        logger = serviceProvider.GetService(typeof(ILogger<EventBusServiceBus>)) as ILogger<EventBusServiceBus>;
        managementClient = new ManagementClient(eventBusConfig.EventBusConnectionString);
        topicClient = CreateTopicClient();
    }

    public override void Publish(IntegrationEvent @event)
    {
        var eventName = @event.GetType().Name; // OrderCreatedIntegrationEvent olarak gelecek ama biz sonundaki IntegrationEvent kısmını keseceğiz.
        eventName = ProcessEventName(eventName); // OrderCreated oldu sonunu kırpmış olduk.
        var eventStr = JsonConvert.SerializeObject(@event);
        var bodyArr = Encoding.UTF8.GetBytes(eventStr);

        var message = new Message()
        {
            MessageId = Guid.NewGuid().ToString(),
            Body = bodyArr,
            Label = eventName
        };

        topicClient.SendAsync(message).GetAwaiter().GetResult();
    }

    public override void Subscribe<T, TH>()
    {
        var eventName = typeof(T).Name;
        eventName = ProcessEventName(eventName);

        if (!SubsManager.HasSubscriptionForEvent(eventName))
        {
            var subscriptionClient = CreateSubscriptionClientIfNotExists(eventName);

            RegisterSubscriptionClientMessageHandler(subscriptionClient);
        }
        logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

        SubsManager.AddSubscription<T, TH>();
    }

    public override void UnSubscribe<T, TH>()
    {
        var eventName = typeof(T).Name;

        try
        {
            var subscriptionClient = CreateSubscriptionClient(eventName);

            subscriptionClient.RemoveRuleAsync(eventName).GetAwaiter().GetResult();
        }
        catch (MessagingEntityNotFoundException)
        {
            logger.LogError("The messaging entity {eventName} could not be found.", eventName);
        }

        logger.LogInformation("UnSubscribing from event {EventName}.", eventName);

        SubsManager.RemoveSubscription<T, TH>();
    }

    public override void Dispose()
    {
        base.Dispose();

        topicClient.CloseAsync().GetAwaiter().GetResult();
        managementClient.CloseAsync().GetAwaiter().GetResult();
        topicClient = null;
        managementClient = null;
    }

    #region Private Methods
    private ITopicClient CreateTopicClient()
    {
        if (topicClient is null || topicClient.IsClosedOrClosing)
        {
            topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);
        }

        if (!managementClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
        {
            managementClient.CreateTopicAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();
        }

        return topicClient;
    }

    private SubscriptionClient CreateSubscriptionClient(string eventName) => new(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));

    private ISubscriptionClient CreateSubscriptionClientIfNotExists(string eventName)
    {
        var subClient = CreateSubscriptionClient(eventName);

        var exists = managementClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();

        if (!exists)
        {
            managementClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
            RemoveDefaultRule(subClient);
        }

        CreateRuleIfNotExists(ProcessEventName(eventName), subClient);

        return subClient;
    }

    private void CreateRuleIfNotExists(string eventName, ISubscriptionClient subscriptionClient)
    {
        bool ruleExists;

        try
        {
            var rule = managementClient.GetRuleAsync(EventBusConfig.DefaultTopicName, eventName, eventName).GetAwaiter().GetResult();
            ruleExists = rule is not null;
        }
        catch (MessagingEntityNotFoundException)
        {
            // azure servicebus tarafında rule olmadığı anlamına gelir buraya düşerse
            ruleExists = false;
        }

        if (!ruleExists)
        {
            subscriptionClient.AddRuleAsync(new()
            {
                Name = eventName,
                Filter = new CorrelationFilter() { Label = eventName } // gelen mesajın bize gelip gelmediğini anlamak için filter kullandık.
            }).GetAwaiter().GetResult();
        }
    }

    private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
    {
        try
        {
            subscriptionClient
                .RemoveRuleAsync(RuleDescription.DefaultRuleName)
                .GetAwaiter()
                .GetResult();
        }
        catch (MessagingEntityNotFoundException)
        {
            logger.LogError("The messaging entity {DefaultRuleName} Could not be found", RuleDescription.DefaultRuleName);
        }
    }

    private void RegisterSubscriptionClientMessageHandler(ISubscriptionClient subscriptionClient)
    { // subscription altına herhangibir mesaj iletildiğinde bu fonksiyon çalışacak.
        subscriptionClient.RegisterMessageHandler(
            async (message, token) =>
            {
                var eventName = $"{message.Label}";
                var messageData = Encoding.UTF8.GetString(message.Body);

                if (await ProcessEvent(ProcessEventName(eventName), messageData))
                {
                    await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                }
            },
            new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 10,
                AutoComplete = false
            });
    }

    private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
    {
        var ex = exceptionReceivedEventArgs.Exception;
        var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

        logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

        return Task.CompletedTask;
    }
    #endregion

}

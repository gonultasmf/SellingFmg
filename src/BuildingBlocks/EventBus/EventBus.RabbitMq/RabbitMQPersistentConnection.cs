using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace EventBus.RabbitMq;

public class RabbitMQPersistentConnection : IDisposable
{
    private IConnection connection;
    private readonly IConnectionFactory connectionFactory;
    private readonly int retyCount;
    private object lock_object = new object();
    private bool _disposed;

    public bool IsConnected => connection is not null && connection.IsOpen;

    public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retyCount = 5)
    {
        this.connectionFactory = connectionFactory;
        this.retyCount = retyCount;
    }

    public IModel CreateModel()
    {
        return connection.CreateModel();
    }

    public void Dispose()
    {
        _disposed = true;
        connection?.Dispose();
    }

    public bool TryConnect()
    {
        lock (lock_object)
        {
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(retyCount, x => TimeSpan.FromSeconds(Math.Pow(2, x)), (ex, time) =>
                {
                });

            policy.Execute(() =>
            {
                connection = connectionFactory.CreateConnection();
            });

            if (IsConnected)
            {
                connection.ConnectionShutdown += Connection_ConnectionShutdown;
                connection.CallbackException += Connection_CallbackException;
                connection.ConnectionBlocked += Connection_ConnectionBlocked;
                
                // log basılabilir.
                return true;
            }

            return false;
        }
    }

    
    private void Connection_ConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        // log atılabilir. (Bağlantı kopma durumunda buraya düşecektir.)
        if (_disposed) return;

        TryConnect();
    }

    private void Connection_ConnectionBlocked(object? sender, RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
    {
        if (_disposed) return;

        TryConnect();
    }

    private void Connection_CallbackException(object? sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
    {
        if (_disposed) return;

        TryConnect();
    }

}

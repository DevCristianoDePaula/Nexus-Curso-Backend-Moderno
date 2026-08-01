using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Nexus.Shared.Messaging.Bus;

/// <summary>
/// Implementação concreta do IMessageBus usando RabbitMQ.
/// Gerencia a conexão, o canal e as operações de publish/subscribe.
/// O exchange "nexus-exchange" do tipo Topic roteia mensagens pelo
/// nome do tipo (routing key = typeof(T).Name).
/// Nota: em produção, usar IConnectionMultiplexer e reconexão automática.
/// </summary>
public class RabbitMqBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqBus(IOptions<RabbitMqOptions> options)
    {
        // Configura a ConnectionFactory com host, porta e credenciais
        var factory = new ConnectionFactory
        {
            HostName = options.Value.Host,
            Port = options.Value.Port,
            UserName = options.Value.Username,
            Password = options.Value.Password
        };
        // Cria conexão e canal (bloqueante para simplificar — preferir async em produção)
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        // Declara o exchange principal do tipo Topic com persistência
        _channel.ExchangeDeclareAsync("nexus-exchange", ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Publica uma mensagem no exchange "nexus-exchange".
    /// A routing key é o nome do tipo (ex: "OrderSubmittedEvent").
    /// </summary>
    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        var routingKey = typeof(T).Name;
        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: "nexus-exchange",
            routingKey: routingKey,
            body: body);
    }

    /// <summary>
    /// Inscreve um handler em uma fila. A fila é ligada ao exchange
    /// usando o nome do tipo como routing key. O consumer faz auto-ack
    /// manual (precisa chamar BasicAck após processar).
    /// </summary>
    public async Task SubscribeAsync<T>(string queue, Func<T, Task> handler, CancellationToken ct = default) where T : class
    {
        await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(queue, "nexus-exchange", typeof(T).Name);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var message = JsonConvert.DeserializeObject<T>(json);
            if (message is not null)
                await handler(message);
            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        };

        await _channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
    }
}

/// <summary>
/// Opções de configuração do RabbitMQ (lidas do appsettings.json).
/// Valores padrão para desenvolvimento local.
/// </summary>
public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "nexus";
    public string Password { get; set; } = "Nexus@2026#";
}

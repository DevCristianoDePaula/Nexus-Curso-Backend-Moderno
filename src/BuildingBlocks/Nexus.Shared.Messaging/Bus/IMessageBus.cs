namespace Nexus.Shared.Messaging.Bus;

/// <summary>
/// Interface do barramento de mensagens (Message Bus).
/// Abstrai a comunicação assíncrona entre serviços via RabbitMQ.
/// Publisher: envia mensagens para o exchange.
/// Consumer: assina filas e processa mensagens recebidas.
/// </summary>
public interface IMessageBus
{
    /// <summary>Publica uma mensagem no barramento (exchange padrão).</summary>
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;

    /// <summary>Inscreve um handler em uma fila para processar mensagens.</summary>
    Task SubscribeAsync<T>(string queue, Func<T, Task> handler, CancellationToken ct = default) where T : class;
}

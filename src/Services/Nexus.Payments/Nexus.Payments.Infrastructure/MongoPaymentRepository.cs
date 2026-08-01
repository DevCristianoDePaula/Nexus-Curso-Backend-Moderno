using MongoDB.Driver;
using Nexus.Payments.Application;
using Nexus.Payments.Domain;

namespace Nexus.Payments.Infrastructure;

///
/// <summary>
/// Repositório de Pagamentos implementado com MongoDB.
/// 
/// Padrão **Repository**: abstrai o acesso a dados da collection "payments".
/// </summary>
public class MongoPaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Payment> _collection;

    public MongoPaymentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Payment>("payments");
    }

    ///
    /// <summary>
    /// Busca um pagamento pelo ID.
    /// </summary>
    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cursor = await _collection.FindAsync(p => p.Id == id, cancellationToken: ct);
        return await cursor.FirstOrDefaultAsync(ct);
    }

    ///
    /// <summary>
    /// Busca um pagamento associado a um pedido.
    /// </summary>
    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var cursor = await _collection.FindAsync(p => p.OrderId == orderId, cancellationToken: ct);
        return await cursor.FirstOrDefaultAsync(ct);
    }

    ///
    /// <summary>
    /// Insere um novo pagamento na collection.
    /// </summary>
    public async Task CreateAsync(Payment payment, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(payment, cancellationToken: ct);
    }

    ///
    /// <summary>
    /// Substitui (ReplaceOne) um pagamento existente.
    /// </summary>
    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        await _collection.ReplaceOneAsync(p => p.Id == payment.Id, payment, cancellationToken: ct);
    }
}

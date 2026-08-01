using MongoDB.Driver;
using Nexus.Coupons.Application;
using Nexus.Coupons.Domain;

namespace Nexus.Coupons.Infrastructure;

///
/// <summary>
/// Repositório de Cupons implementado com MongoDB.
/// 
/// Padrão **Repository**: abstrai o acesso a dados da collection "coupons".
/// Garante unicidade do campo "Code" através de um índice único criado no construtor.
/// </summary>
public class MongoCouponRepository : ICouponRepository
{
    private readonly IMongoCollection<Coupon> _collection;

    public MongoCouponRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Coupon>("coupons");

        // Cria um índice único ascendente no campo Code para garantir
        // que não haja dois cupons com o mesmo código (consistência a nível de banco).
        var index = new CreateIndexModel<Coupon>(
            Builders<Coupon>.IndexKeys.Ascending(c => c.Code),
            new CreateIndexOptions { Unique = true });
        _collection.Indexes.CreateOne(index);
    }

    ///
    /// <summary>
    /// Busca um cupom pelo código (case-sensitive — usar maiúsculas na camada de aplicação).
    /// </summary>
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var cursor = await _collection.FindAsync(c => c.Code == code, cancellationToken: ct);
        return await cursor.FirstOrDefaultAsync(ct);
    }

    ///
    /// <summary>
    /// Insere um novo cupom na collection.
    /// </summary>
    public async Task CreateAsync(Coupon coupon, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(coupon, cancellationToken: ct);
    }

    ///
    /// <summary>
    /// Substitui (ReplaceOne) um cupom existente.
    /// </summary>
    public async Task UpdateAsync(Coupon coupon, CancellationToken ct = default)
    {
        await _collection.ReplaceOneAsync(c => c.Id == coupon.Id, coupon, cancellationToken: ct);
    }
}

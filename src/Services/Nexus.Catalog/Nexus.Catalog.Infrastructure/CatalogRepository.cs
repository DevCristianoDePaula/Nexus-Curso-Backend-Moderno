using MongoDB.Driver;
using Nexus.Catalog.Application;
using Nexus.Catalog.Domain;

namespace Nexus.Catalog.Infrastructure;

///
/// <summary>
/// Repositório de Catálogo implementado com MongoDB.
/// 
/// Padrão **Repository**: abstrai o acesso a dados da collection "products" no MongoDB.
/// A injeção de IMongoDatabase permite queries tipadas com filtros e builders do driver nativo.
/// 
/// Cada método mapeia operações de domínio para operações MongoDB:
/// - GetByIdAsync → Find + FirstOrDefault
/// - GetByCategoryAsync / SearchAsync → Find com Skip/Limit (paginação)
/// - CreateAsync → InsertOne
/// - UpdateAsync → ReplaceOne
/// </summary>
public class CatalogRepository : ICatalogRepository
{
    private readonly IMongoCollection<Product> _products;

    public CatalogRepository(IMongoDatabase database)
    {
        _products = database.GetCollection<Product>("products");
    }

    ///
    /// <summary>
    /// Busca um produto pelo ID.
    /// </summary>
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        return await _products.Find(filter).FirstOrDefaultAsync(ct);
    }

    ///
    /// <summary>
    /// Retorna produtos de uma categoria com paginação.
    /// </summary>
    public async Task<List<Product>> GetByCategoryAsync(string categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
        return await _products.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    ///
    /// <summary>
    /// Pesquisa produtos pelo nome usando regex case-insensitive.
    /// </summary>
    public async Task<List<Product>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken ct = default)
    {
        // Expressão regular case-insensitive (opção "i") para busca textual no MongoDB.
        var filter = Builders<Product>.Filter.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
        return await _products.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    ///
    /// <summary>
    /// Insere um novo produto na collection.
    /// </summary>
    public async Task CreateAsync(Product product, CancellationToken ct = default)
    {
        await _products.InsertOneAsync(product, cancellationToken: ct);
    }

    ///
    /// <summary>
    /// Substitui (ReplaceOne) o documento do produto — equivalente a um UPDATE completo no MongoDB.
    /// </summary>
    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
        await _products.ReplaceOneAsync(filter, product, cancellationToken: ct);
    }

    ///
    /// <summary>
    /// Retorna a contagem total de produtos na collection.
    /// </summary>
    public async Task<long> GetCountAsync(CancellationToken ct = default)
    {
        return await _products.CountDocumentsAsync(FilterDefinition<Product>.Empty, cancellationToken: ct);
    }
}
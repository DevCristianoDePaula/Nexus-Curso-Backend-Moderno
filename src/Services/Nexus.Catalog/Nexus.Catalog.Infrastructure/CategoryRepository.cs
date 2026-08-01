using MongoDB.Driver;
using Nexus.Catalog.Application;
using Nexus.Catalog.Domain;

namespace Nexus.Catalog.Infrastructure;

///
/// <summary>
/// Repositório de Categorias implementado com MongoDB.
/// 
/// Padrão **Repository**: abstrai o acesso a dados da collection "categories",
/// ordenando por DisplayOrder para manter a ordem de exibição definida pelo domínio.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryRepository(IMongoDatabase database)
    {
        _categories = database.GetCollection<Category>("categories");
    }

    ///
    /// <summary>
    /// Retorna todas as categorias ordenadas por DisplayOrder.
    /// </summary>
    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
    {
        // FilterDefinition.Empty → busca todos os documentos sem filtro.
        return await _categories.Find(FilterDefinition<Category>.Empty)
            .SortBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    ///
    /// <summary>
    /// Busca uma categoria pelo ID (string).
    /// </summary>
    public async Task<Category?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, id);
        return await _categories.Find(filter).FirstOrDefaultAsync(ct);
    }

    ///
    /// <summary>
    /// Insere uma nova categoria.
    /// </summary>
    public async Task CreateAsync(Category category, CancellationToken ct = default)
    {
        await _categories.InsertOneAsync(category, cancellationToken: ct);
    }

    ///
    /// <summary>
    /// Substitui (ReplaceOne) uma categoria existente.
    /// </summary>
    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, category.Id);
        await _categories.ReplaceOneAsync(filter, category, cancellationToken: ct);
    }
}
using Nexus.Catalog.Domain;

namespace Nexus.Catalog.Application;

///
/// <summary>
/// DTO (Data Transfer Object) para a criação de um novo produto.
/// Representa os dados de entrada da API — desacoplando a camada de transporte do domínio.
/// </summary>
public class CreateProductRequest
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Price { get; init; }
    public string Currency { get; init; } = "BRL";
    public string CategoryId { get; init; } = "";
    public string CategoryName { get; init; } = "";
    public string SellerId { get; init; } = "";
    public string SellerName { get; init; } = "";
    public string Sku { get; init; } = "";
    public int StockQuantity { get; init; }
    public Dictionary<string, string>? Specifications { get; init; }
}

///
/// <summary>
/// DTO de resposta que representa um produto para o cliente da API.
/// O método estático <see cref="From"/> converte a entidade de domínio em um contrato público,
/// garantindo que detalhes internos do domínio não vazem para fora.
/// </summary>
public class ProductResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Price { get; init; }
    public string Currency { get; init; } = "";
    public string CategoryId { get; init; } = "";
    public string CategoryName { get; init; } = "";
    public string SellerName { get; init; } = "";
    public string Sku { get; init; } = "";
    public int StockQuantity { get; init; }
    public List<string> ImageUrls { get; init; } = [];
    public string Status { get; init; } = "";
    public DateTime CreatedAt { get; init; }

    public static ProductResponse From(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price.Amount,
        Currency = product.Price.Currency,
        CategoryId = product.CategoryId,
        CategoryName = product.CategoryName,
        SellerName = product.SellerName,
        Sku = product.Sku.Value,
        StockQuantity = product.StockQuantity,
        ImageUrls = product.ImageUrls,
        Status = product.Status.ToString(),
        CreatedAt = product.CreatedAt
    };
}

///
/// <summary>
/// Serviço de aplicação do Catálogo.
/// Orquestra as regras de negócio (Domínio) e a persistência (Infraestrutura)
/// seguindo o padrão **Application Service** do DDD — uma fachada fina que coordena operações.
/// Depende de interfaces de repositório (ICatalogRepository, ICategoryRepository),
/// nunca de implementações concretas (Inversão de Dependência).
/// </summary>
public class CatalogService
{
    private readonly ICatalogRepository _catalog;
    private readonly ICategoryRepository _categories;

    // Injeção de dependência via construtor — as dependências são resolvidas pelo container DI.
    public CatalogService(ICatalogRepository catalog, ICategoryRepository categories)
    {
        _catalog = catalog;
        _categories = categories;
    }

    ///
    /// <summary>
    /// Cria um novo produto validando que a categoria informada existe.
    /// Retorna um <see cref="ProductResponse"/> para não expor a entidade de domínio.
    /// </summary>
    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var category = await _categories.GetByIdAsync(request.CategoryId, ct)
            ?? throw new InvalidOperationException($"Category {request.CategoryId} not found");

        var product = new Product(
            request.Name,
            request.Description,
            new Money(request.Price, request.Currency),
            category.Id,
            category.Name,
            request.SellerId,
            request.SellerName,
            new Domain.Sku(request.Sku),
            request.StockQuantity,
            request.Specifications);

        await _catalog.CreateAsync(product, ct);
        return ProductResponse.From(product);
    }

    ///
    /// <summary>
    /// Busca um produto pelo ID. Retorna null quando não encontrado.
    /// </summary>
    public async Task<ProductResponse?> GetProductAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _catalog.GetByIdAsync(id, ct);
        return product is null ? null : ProductResponse.From(product);
    }

    ///
    /// <summary>
    /// Retorna produtos de uma categoria específica com paginação.
    /// </summary>
    public async Task<List<ProductResponse>> GetByCategoryAsync(string categoryId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var products = await _catalog.GetByCategoryAsync(categoryId, page, pageSize, ct);
        return products.Select(ProductResponse.From).ToList();
    }

    ///
    /// <summary>
    /// Pesquisa produtos por termo textual com paginação (busca case-insensitive no MongoDB).
    /// </summary>
    public async Task<List<ProductResponse>> SearchAsync(string term, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var products = await _catalog.SearchAsync(term, page, pageSize, ct);
        return products.Select(ProductResponse.From).ToList();
    }
}
namespace Nexus.Catalog.Domain;

/// <summary>
/// Agregado raiz do catálogo. Representa um produto à venda na plataforma.
/// Contém regras de negócio para validação de nome, preço, estoque e imagens.
/// No DDD, esta é a entidade central do domínio de Catálogo e garante a consistência
/// de todas as suas propriedades através de métodos de comportamento (não setters públicos).
/// </summary>
public sealed class Product
{
    // Identificador único do produto (UUID)
    public Guid Id { get; private set; }

    // Nome do produto — limite de 200 caracteres
    public string Name { get; private set; }

    // Descrição detalhada — limite de 5000 caracteres
    public string Description { get; private set; }

    // Preço atual representado como Value Object (Money) para evitar erros de moeda
    public Money Price { get; private set; }

    // Chave estrangeira para a categoria (desnormalizada para consulta rápida)
    public string CategoryId { get; private set; }

    // Nome da categoria no momento da criação (desnormalização intencional)
    public string CategoryName { get; private set; }

    // Identificador do vendedor (dono do produto)
    public string SellerId { get; private set; }

    // Nome do vendedor no momento da criação (desnormalização intencional)
    public string SellerName { get; private set; }

    // SKU (Stock Keeping Unit) — Value Object com validação de formato
    public Sku Sku { get; private set; }

    // Quantidade disponível em estoque (não pode ser negativa)
    public int StockQuantity { get; private set; }

    // URLs das imagens do produto (máximo 10)
    public List<string> ImageUrls { get; private set; }

    // Especificações técnicas genéricas (ex: "Cor", "Tamanho") — chave/valor flexível
    public Dictionary<string, string> Specifications { get; private set; }

    // Status do produto: ativo ou inativo (define visibilidade na loja)
    public ProductStatus Status { get; private set; }

    // Timestamp de criação (definido uma única vez no construtor)
    public DateTime CreatedAt { get; private set; }

    // Timestamp da última alteração (atualizado por todo método de modificação)
    public DateTime UpdatedAt { get; private set; }

    // Construtor privado exigido pelo Entity Framework para materialização de proxies
    private Product() { }

    /// <summary>
    /// Construtor principal. Cria um produto ativo com os dados fornecidos.
    /// Utiliza os métodos de comportamento (Set*) para aplicar as regras de validação,
    /// garantindo que o objeto nunca seja criado em estado inconsistente.
    /// </summary>
    public Product(
        string name,
        string description,
        Money price,
        string categoryId,
        string categoryName,
        string sellerId,
        string sellerName,
        Sku sku,
        int stockQuantity,
        Dictionary<string, string>? specifications = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        SetCategory(categoryId, categoryName);
        SetSeller(sellerId, sellerName);
        Sku = sku ?? throw new ArgumentNullException(nameof(sku));
        SetStock(stockQuantity);
        ImageUrls = [];
        Specifications = specifications ?? [];
        Status = ProductStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    /// <summary>
    /// Altera o nome do produto com validação de tamanho máximo.
    /// </summary>
    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        if (name.Length > 200) throw new ArgumentException("Product name must not exceed 200 characters", nameof(name));
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Altera a descrição com validação de tamanho máximo.
    /// </summary>
    public void SetDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        if (description.Length > 5000) throw new ArgumentException("Description must not exceed 5000 characters", nameof(description));
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Altera o preço. O Value Object Money já garante que o valor não seja negativo.
    /// </summary>
    public void SetPrice(Money price)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Altera a categoria do produto. Ambos Id e Nome são atualizados juntos para manter consistência.
    /// </summary>
    public void SetCategory(string categoryId, string categoryName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryId, nameof(categoryId));
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName, nameof(categoryName));
        CategoryId = categoryId;
        CategoryName = categoryName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Altera o vendedor responsável pelo produto.
    /// </summary>
    public void SetSeller(string sellerId, string sellerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sellerId, nameof(sellerId));
        ArgumentException.ThrowIfNullOrWhiteSpace(sellerName, nameof(sellerName));
        SellerId = sellerId;
        SellerName = sellerName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Define a quantidade em estoque. Não permite valores negativos pois
    /// estoque negativo não faz sentido no domínio.
    /// </summary>
    public void SetStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reduz o estoque ao confirmar uma compra. Garante que não seja possível
    /// vender mais do que o disponível (regra de negócio fundamental).
    /// </summary>
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Decrease quantity must be positive", nameof(quantity));
        if (StockQuantity < quantity) throw new InvalidOperationException($"Insufficient stock: available {StockQuantity}, requested {quantity}");
        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adiciona uma URL de imagem. Limitado a 10 imagens por produto para
    /// evitar abuso de armazenamento e degradação de performance.
    /// </summary>
    public void AddImage(string imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl, nameof(imageUrl));
        if (ImageUrls.Count >= 10) throw new InvalidOperationException("Maximum of 10 images per product");
        ImageUrls.Add(imageUrl);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adiciona ou atualiza uma especificação técnica (chave/valor).
    /// Útil para filtros de busca e comparação entre produtos.
    /// </summary>
    public void AddSpecification(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        Specifications[key] = value ?? throw new ArgumentNullException(nameof(value));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desativa o produto (oculta da vitrine). Não remove do banco — soft delete.
    /// </summary>
    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reativa o produto, tornando-o visível novamente.
    /// </summary>
    public void Activate()
    {
        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Enum que define os estados possíveis de um produto no catálogo.
/// Apenas produtos ativos são exibidos na loja.
/// </summary>
public enum ProductStatus
{
    Active,
    Inactive
}
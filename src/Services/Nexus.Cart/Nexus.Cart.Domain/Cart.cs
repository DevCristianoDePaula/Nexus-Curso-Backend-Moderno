namespace Nexus.Cart.Domain;

/// <summary>
/// Agregado raiz do domínio de Carrinho. Representa o carrinho de compras
/// de um usuário, contendo itens e regras para adicionar, remover e atualizar
/// quantidades. Diferente de um pedido, o carrinho é temporário e não possui
/// validações complexas de estado — é um agregado simplificado (não herda AggregateRoot).
/// </summary>
public class Cart
{
    // ID do usuário dono do carrinho (chave estrangeira para NexusUser)
    public string UserId { get; private set; }

    // Lista de itens no carrinho
    public List<CartItem> Items { get; private set; }

    // Timestamp da última modificação (usado para detectar carrinhos abandonados)
    public DateTime LastUpdated { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private Cart() { }

    /// <summary>
    /// Cria um carrinho vazio para o usuário informado.
    /// </summary>
    public Cart(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Items = [];
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Adiciona um item ao carrinho. Se o produto já existe, apenas incrementa
    /// a quantidade (evita duplicatas do mesmo produto no carrinho).
    /// </summary>
    public void AddItem(string productId, string productName, decimal price, string currency, int quantity = 1)
    {
        // Se o produto já está no carrinho, apenas aumenta a quantidade
        var existing = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.AddQuantity(quantity);
        }
        else
        {
            Items.Add(new CartItem(productId, productName, price, currency, quantity));
        }
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove completamente um item do carrinho pelo ID do produto.
    /// </summary>
    public void RemoveItem(string productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza a quantidade de um item. Se a quantidade for zero ou negativa,
    /// o item é removido — isso simplifica a lógica do cliente (não precisa
    /// chamar RemoveItem separadamente).
    /// </summary>
    public void UpdateQuantity(string productId, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveItem(productId);
            return;
        }
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            item.SetQuantity(quantity);
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Limpa todos os itens do carrinho (usado após finalizar o pedido).
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        LastUpdated = DateTime.UtcNow;
    }

    // Total calculado — soma dos subtotais de cada item (propriedade computada)
    public decimal Total => Items.Sum(i => i.Subtotal);
}

/// <summary>
/// Item individual dentro do carrinho. Value Object que armazena o snapshot
/// do produto no momento em que foi adicionado ao carrinho.
/// O subtotal é calculado automaticamente (UnitPrice * Quantity).
/// </summary>
public class CartItem
{
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; }
    public int Quantity { get; private set; }

    // Subtotal calculado (propriedade somente leitura computada)
    public decimal Subtotal => UnitPrice * Quantity;

    public CartItem(string productId, string productName, decimal unitPrice, string currency, int quantity)
    {
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("Price cannot be negative");
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be positive");
    }

    // Incrementa a quantidade (usado quando o mesmo produto é adicionado novamente)
    public void AddQuantity(int quantity) => Quantity += quantity;

    // Define a quantidade exata (usado na atualização manual pelo usuário)
    public void SetQuantity(int quantity) => Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be positive");
}

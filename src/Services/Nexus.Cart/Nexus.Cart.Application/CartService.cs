namespace Nexus.Cart.Application;

///
/// <summary>
/// Serviço de aplicação do Carrinho de Compras.
/// Gerencia as operações sobre o carrinho de um usuário: adicionar, remover, atualizar e limpar itens.
/// 
/// Padrões aplicados:
/// - **Application Service**: orquestra o domínio (Cart) e a infraestrutura (ICartRepository).
/// - **Repository Pattern**: abstrai o armazenamento (Redis) por trás de uma interface.
/// - **DTO**: AddCartItemRequest desacopla a entrada da API.
/// </summary>
public class CartService
{
    private readonly ICartRepository _repository;

    public CartService(ICartRepository repository) => _repository = repository;

    ///
    /// <summary>
    /// Obtém o carrinho do usuário. Se não existir, retorna um novo carrinho vazio.
    /// </summary>
    public async Task<Domain.Cart> GetCartAsync(string userId, CancellationToken ct = default)
    {
        return await _repository.GetAsync(userId, ct) ?? new Domain.Cart(userId);
    }

    ///
    /// <summary>
    /// Adiciona um item ao carrinho do usuário. Cria um novo carrinho se necessário.
    /// </summary>
    public async Task AddItemAsync(string userId, AddCartItemRequest request, CancellationToken ct = default)
    {
        var cart = await _repository.GetAsync(userId, ct) ?? new Domain.Cart(userId);
        cart.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.Currency, request.Quantity);
        await _repository.SaveAsync(cart, ct);
    }

    ///
    /// <summary>
    /// Remove um item do carrinho do usuário.
    /// </summary>
    public async Task RemoveItemAsync(string userId, string productId, CancellationToken ct = default)
    {
        var cart = await _repository.GetAsync(userId, ct);
        if (cart is null) return;
        cart.RemoveItem(productId);
        await _repository.SaveAsync(cart, ct);
    }

    ///
    /// <summary>
    /// Atualiza a quantidade de um item no carrinho.
    /// </summary>
    public async Task UpdateQuantityAsync(string userId, string productId, int quantity, CancellationToken ct = default)
    {
        var cart = await _repository.GetAsync(userId, ct);
        if (cart is null) return;
        cart.UpdateQuantity(productId, quantity);
        await _repository.SaveAsync(cart, ct);
    }

    ///
    /// <summary>
    /// Limpa (deleta) o carrinho do usuário do repositório.
    /// </summary>
    public async Task ClearCartAsync(string userId, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(userId, ct);
    }
}

///
/// <summary>
/// DTO de entrada para adicionar um item ao carrinho.
/// </summary>
public class AddCartItemRequest
{
    public string ProductId { get; init; } = "";
    public string ProductName { get; init; } = "";
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "BRL";
    public int Quantity { get; init; } = 1;
}

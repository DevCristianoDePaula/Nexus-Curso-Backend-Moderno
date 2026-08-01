using Nexus.Orders.Domain;

namespace Nexus.Orders.Application;

///
/// <summary>
/// Serviço de aplicação de Pedidos.
/// Coordena a criação, consulta, confirmação de pagamento e cancelamento de pedidos.
/// 
/// Padrões aplicados:
/// - **Application Service**: coordena operações entre o domínio (Order) e a infraestrutura.
/// - **Repository Pattern**: IOrderRepository abstrai o EF Core.
/// - **Domain Events**: DomainEventDispatcher publica eventos de domínio após ações importantes.
/// - **DTOs**: CreateOrderRequest / CreateOrderItemRequest desacoplam a API do domínio.
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly DomainEventDispatcher _eventDispatcher;

    public OrderService(IOrderRepository repository, DomainEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    ///
    /// <summary>
    /// Cria um novo pedido a partir dos dados fornecidos.
    /// Monta o endereço de entrega, adiciona os itens, aplica cupom (se houver),
    /// submete o pedido, persiste e dispara eventos de domínio.
    /// </summary>
    public async Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var address = new Address(
            request.Street, request.Number, request.Neighborhood,
            request.City, request.State, request.ZipCode,
            request.Complement);

        var order = new Order(request.CustomerId, address);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.ImageUrl);
        }

        if (!string.IsNullOrEmpty(request.CouponCode) && request.DiscountAmount.HasValue)
        {
            order.ApplyCoupon(request.CouponCode, request.DiscountAmount.Value);
        }

        order.Submit();
        await _repository.CreateAsync(order, ct);
        await _eventDispatcher.DispatchAsync(order, ct);
        return order;
    }

    ///
    /// <summary>
    /// Obtém um pedido pelo ID. Retorna null se não encontrado.
    /// </summary>
    public async Task<Order?> GetOrderAsync(Guid id, CancellationToken ct = default)
    {
        return await _repository.GetByIdAsync(id, ct);
    }

    ///
    /// <summary>
    /// Lista os pedidos de um cliente com paginação (ordenados do mais recente para o mais antigo).
    /// </summary>
    public async Task<List<Order>> GetCustomerOrdersAsync(string customerId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _repository.GetByCustomerAsync(customerId, page, pageSize, ct);
    }

    ///
    /// <summary>
    /// Confirma o pagamento de um pedido. Atualiza o status e dispara eventos de domínio.
    /// </summary>
    public async Task<Order?> ConfirmPaymentAsync(Guid orderId, string paymentId, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(orderId, ct);
        if (order is null) return null;
        order.ConfirmPayment(paymentId);
        await _repository.UpdateAsync(order, ct);
        await _eventDispatcher.DispatchAsync(order, ct);
        return order;
    }

    ///
    /// <summary>
    /// Cancela um pedido com um motivo. Atualiza o status e dispara eventos de domínio.
    /// </summary>
    public async Task<Order?> CancelOrderAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(orderId, ct);
        if (order is null) return null;
        order.Cancel(reason);
        await _repository.UpdateAsync(order, ct);
        await _eventDispatcher.DispatchAsync(order, ct);
        return order;
    }
}

///
/// <summary>
/// DTO de entrada para criação de um pedido.
/// Contém dados do cliente, endereço de entrega, cupom e lista de itens.
/// </summary>
public class CreateOrderRequest
{
    public string CustomerId { get; init; } = "";
    public string Street { get; init; } = "";
    public string Number { get; init; } = "";
    public string? Complement { get; init; }
    public string Neighborhood { get; init; } = "";
    public string City { get; init; } = "";
    public string State { get; init; } = "";
    public string ZipCode { get; init; } = "";
    public string? CouponCode { get; init; }
    public decimal? DiscountAmount { get; init; }
    public List<CreateOrderItemRequest> Items { get; init; } = [];
}

///
/// <summary>
/// DTO de entrada para um item dentro do pedido.
/// </summary>
public class CreateOrderItemRequest
{
    public string ProductId { get; init; } = "";
    public string ProductName { get; init; } = "";
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? ImageUrl { get; init; }
}

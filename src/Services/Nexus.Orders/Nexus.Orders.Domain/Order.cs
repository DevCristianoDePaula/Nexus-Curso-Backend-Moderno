using Nexus.Shared.Kernel.Entities;

namespace Nexus.Orders.Domain;

/// <summary>
/// Agregado raiz do domínio de Pedidos. Representa um pedido de compra
/// com seu ciclo de vida completo: Pendente → Submetido → Pago → Enviado → Entregue.
/// Cada transição de estado é protegida por invariantes que garantem a consistência
/// do domínio. Eventos de domínio são disparados nas transições importantes
/// para que outros serviços (Pagamentos, Notificações) reajam adequadamente.
/// </summary>
public class Order : AggregateRoot
{
    // ID do cliente que fez o pedido
    public string CustomerId { get; private set; }

    // Nome do cliente no momento do pedido (desnormalizado para consultas)
    public string? CustomerName { get; private set; }

    // Endereço de entrega do pedido (Value Object imutável)
    public Address ShippingAddress { get; private set; }

    // Itens do pedido (cada item é um snapshot do produto no momento da compra)
    public List<OrderItem> Items { get; private set; }

    // Valor total do pedido (soma dos itens menos desconto)
    public decimal TotalAmount { get; private set; }

    // Moeda do pedido (ex: BRL, USD)
    public string Currency { get; private set; }

    // Estado atual do pedido no fluxo de cumprimento
    public OrderStatus Status { get; private set; }

    // ID do pagamento associado (preenchido após confirmação)
    public string? PaymentId { get; private set; }

    // Código do cupom aplicado (se houver)
    public string? CouponCode { get; private set; }

    // Valor do desconto aplicado pelo cupom
    public decimal? DiscountAmount { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private Order() { }

    /// <summary>
    /// Cria um novo pedido com status Pendente. O pedido começa vazio
    /// e itens são adicionados posteriormente via AddItem.
    /// </summary>
    public Order(string customerId, Address shippingAddress, string currency = "BRL")
    {
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        Items = [];
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Status = OrderStatus.Pending;
        TotalAmount = 0;
        Touch();
    }

    /// <summary>
    /// Adiciona um item ao pedido. Apenas permitido enquanto o pedido está Pendente.
    /// O preço é copiado para o item (snapshot) para refletir o valor no momento da compra.
    /// </summary>
    public void AddItem(string productId, string productName, decimal unitPrice, int quantity, string? imageUrl = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify a non-pending order");

        var item = new OrderItem(productId, productName, unitPrice, Currency, quantity, imageUrl);
        Items.Add(item);
        RecalculateTotal();
        Touch();
    }

    /// <summary>
    /// Aplica um cupom de desconto ao pedido. Valida se o desconto não ultrapassa
    /// o valor total e só permite aplicação em pedidos Pendentes.
    /// </summary>
    public void ApplyCoupon(string couponCode, decimal discountAmount)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot apply coupon to a non-pending order");
        if (discountAmount < 0 || discountAmount > TotalAmount)
            throw new ArgumentException("Invalid discount amount");

        CouponCode = couponCode;
        DiscountAmount = discountAmount;
        RecalculateTotal();
        Touch();
    }

    /// <summary>
    /// Submete o pedido para processamento. Valida se há ao menos um item
    /// (pedido vazio não faz sentido) e se o status atual permite a transição.
    /// Dispara OrderSubmittedEvent para notificar outros serviços.
    /// </summary>
    public void Submit()
    {
        if (Items.Count == 0)
            throw new InvalidOperationException("Cannot submit an empty order");
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order is not in pending status");

        Status = OrderStatus.Submitted;
        Touch();
        AddDomainEvent(new OrderSubmittedEvent(Id, CustomerId, TotalAmount));
    }

    /// <summary>
    /// Confirma o pagamento do pedido. Transição: Submitted → Paid.
    /// Dispara OrderPaidEvent para iniciar o processo de separação/envio.
    /// </summary>
    public void ConfirmPayment(string paymentId)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException("Order must be submitted before payment confirmation");

        PaymentId = paymentId ?? throw new ArgumentNullException(nameof(paymentId));
        Status = OrderStatus.Paid;
        Touch();
        AddDomainEvent(new OrderPaidEvent(Id, paymentId));
    }

    /// <summary>
    /// Marca o pedido como enviado. Transição: Paid → Shipped.
    /// </summary>
    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Cannot ship an unpaid order");

        Status = OrderStatus.Shipped;
        Touch();
    }

    /// <summary>
    /// Marca o pedido como entregue. Transição: Shipped → Delivered.
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Order must be shipped before delivery");

        Status = OrderStatus.Delivered;
        Touch();
    }

    /// <summary>
    /// Cancela o pedido por qualquer motivo. Não permite cancelar pedidos
    /// já entregues. Dispara OrderCancelledEvent para reverter ações tomadas.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered order");

        Status = OrderStatus.Cancelled;
        Touch();
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    // Recalcula o total: soma dos subtotais dos itens menos o desconto do cupom
    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Subtotal) - (DiscountAmount ?? 0);
    }
}

/// <summary>
/// Item de pedido (Value Object). Armazena um snapshot do produto no momento
/// da compra — o preço pode mudar no catálogo depois, mas o pedido mantém
/// o valor original. Cada item tem seu próprio Id para rastreamento individual.
/// </summary>
public class OrderItem
{
    // Identificador único do item (permite rastreamento individual)
    public Guid Id { get; private set; }

    // ID do produto no catálogo (chave estrangeira)
    public string ProductId { get; private set; }

    // Nome do produto no momento da compra (snapshot, não busca do catálogo)
    public string ProductName { get; private set; }

    // Preço unitário no momento da compra (snapshot)
    public decimal UnitPrice { get; private set; }

    // Moeda do preço (mesma do pedido)
    public string Currency { get; private set; }

    // Quantidade comprada
    public int Quantity { get; private set; }

    // URL da imagem do produto (opcional, para exibição no resumo do pedido)
    public string? ImageUrl { get; private set; }

    // Subtotal calculado (UnitPrice * Quantity)
    public decimal Subtotal => UnitPrice * Quantity;

    // Construtor privado exigido pelo Entity Framework
    private OrderItem() { }

    public OrderItem(string productId, string productName, decimal unitPrice, string currency, int quantity, string? imageUrl = null)
    {
        Id = Guid.NewGuid();
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("Price cannot be negative");
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be positive");
        ImageUrl = imageUrl;
    }
}

/// <summary>
/// Value Object que representa um endereço de entrega.
/// Imutável e autovalidável — garante que todos os campos obrigatórios
/// estejam presentes na criação. Não possui lógica de negócio além da validação.
/// </summary>
public class Address
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private Address() { }

    public Address(string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        Number = number ?? throw new ArgumentNullException(nameof(number));
        Neighborhood = neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        Complement = complement;
    }
}

/// <summary>
/// Máquina de estados do pedido. Define as transições possíveis:
/// Pending → Submitted → Paid → Shipped → Delivered
/// Ou de qualquer estado (exceto Delivered) → Cancelled
/// </summary>
public enum OrderStatus
{
    Pending,
    Submitted,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

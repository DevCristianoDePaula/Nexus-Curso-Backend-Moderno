using Nexus.Shared.Kernel.Entities;

namespace Nexus.Coupons.Domain;

/// <summary>
/// Agregado raiz do domínio de Cupons. Representa um cupom de desconto
/// que pode ser aplicado a pedidos. Contém todas as regras de negócio para
/// validação de elegibilidade (data, uso mínimo, categoria, etc.)
/// e cálculo do desconto (percentual ou fixo).
/// </summary>
public class Coupon : AggregateRoot
{
    // Código do cupom (normalizado em MAIÚSCULO — ex: "BLACKFRIDAY")
    public string Code { get; private set; }

    // Descrição amigável do cupom para exibição ao cliente
    public string Description { get; private set; }

    // Tipo de desconto: Percentual (ex: 10%) ou Fixo (ex: R$ 20,00)
    public DiscountType Type { get; private set; }

    // Valor do desconto: percentual (1-100) ou valor fixo em reais
    public decimal Value { get; private set; }

    // Valor mínimo de compra para o cupom ser válido (null = sem mínimo)
    public decimal? MinPurchaseAmount { get; private set; }

    // Número máximo de usos total do cupom (null = ilimitado)
    public int? MaxUses { get; private set; }

    // Quantas vezes o cupom já foi usado (contador incremental)
    public int CurrentUses { get; private set; }

    // Número máximo de usos por cliente (null = sem restrição)
    public int? MaxUsesPerCustomer { get; private set; }

    // Data início de validade (null = começa imediatamente)
    public DateTime? ValidFrom { get; private set; }

    // Data fim de validade (null = nunca expira)
    public DateTime? ValidTo { get; private set; }

    // Indica se o cupom está ativo (cupons inativos não são válidos)
    public bool IsActive { get; private set; }

    // Categoria à qual o cupom se aplica (null = todas as categorias)
    public string? ApplicableCategoryId { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private Coupon() { }

    /// <summary>
    /// Cria um novo cupom ativo com zero usos. O código é normalizado
    /// para maiúsculo para evitar duplicatas como "promo10" e "PROMO10".
    /// </summary>
    public Coupon(string code, string description, DiscountType type, decimal value,
        decimal? minPurchaseAmount = null, int? maxUses = null, int? maxUsesPerCustomer = null,
        DateTime? validFrom = null, DateTime? validTo = null, string? applicableCategoryId = null)
    {
        // Normaliza o código para maiúsculo — evita duplicidade de case
        Code = code?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(code));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Value = value > 0 ? value : throw new ArgumentException("Value must be positive");
        MinPurchaseAmount = minPurchaseAmount;
        MaxUses = maxUses;
        MaxUsesPerCustomer = maxUsesPerCustomer;
        ValidFrom = validFrom;
        ValidTo = validTo;
        ApplicableCategoryId = applicableCategoryId;
        IsActive = true;
        CurrentUses = 0;
    }

    /// <summary>
    /// Verifica se o cupom é válido para um determinado contexto de compra.
    /// Avalia todas as regras de negócio: ativo, data, usos, valor mínimo e categoria.
    /// Retorna true apenas se TODAS as condições forem satisfeitas.
    /// </summary>
    public bool IsValidFor(decimal purchaseAmount, string? customerId = null, string? categoryId = null)
    {
        // Cupom precisa estar ativo
        if (!IsActive) return false;
        // Período de validade (se definido)
        if (ValidFrom.HasValue && DateTime.UtcNow < ValidFrom.Value) return false;
        if (ValidTo.HasValue && DateTime.UtcNow > ValidTo.Value) return false;
        // Limite de usos globais
        if (MaxUses.HasValue && CurrentUses >= MaxUses.Value) return false;
        // Valor mínimo de compra
        if (MinPurchaseAmount.HasValue && purchaseAmount < MinPurchaseAmount.Value) return false;
        // Categoria específica (se o cupom é restrito a uma categoria)
        if (ApplicableCategoryId is not null && categoryId != ApplicableCategoryId) return false;

        return true;
    }

    /// <summary>
    /// Calcula o valor do desconto com base no tipo do cupom.
    /// Percentual: (valor_compra * percentual) / 100
    /// Fixo: valor fixo definido no cupom
    /// O desconto nunca pode ultrapassar o valor total da compra.
    /// </summary>
    public decimal Apply(decimal purchaseAmount)
    {
        if (!IsValidFor(purchaseAmount))
            throw new InvalidOperationException("Coupon is not valid");

        var discount = Type switch
        {
            // Desconto percentual: calcula a porcentagem sobre o valor da compra
            DiscountType.Percentage => purchaseAmount * Value / 100,
            // Desconto fixo: usa o valor direto do cupom
            DiscountType.Fixed => Value,
            _ => 0
        };

        // Garante que o desconto não seja maior que o valor da compra
        return Math.Min(discount, purchaseAmount);
    }

    /// <summary>
    /// Registra o uso do cupom. Deve ser chamado após aplicar o desconto
    /// com sucesso para controlar o limite de usos.
    /// </summary>
    public void Use()
    {
        CurrentUses++;
        Touch();
    }

    /// <summary>
    /// Desativa o cupom manualmente (ex: por suspeita de fraude ou campanha encerrada).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    /// <summary>
    /// Reativa um cupom previamente desativado.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}

/// <summary>
/// Tipo de desconto oferecido pelo cupom.
/// Percentage: desconto percentual (ex: 10% de desconto)
/// Fixed: desconto com valor fixo (ex: R$ 20,00 de desconto)
/// </summary>
public enum DiscountType
{
    Percentage,
    Fixed
}

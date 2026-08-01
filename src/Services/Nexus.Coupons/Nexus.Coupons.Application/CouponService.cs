using Nexus.Coupons.Domain;

namespace Nexus.Coupons.Application;

///
/// <summary>
/// Serviço de aplicação de Cupons de Desconto.
/// Gerencia criação, consulta, validação/aplicação e desativação de cupons.
/// 
/// Padrões aplicados:
/// - **Application Service**: orquestra o domínio (Coupon) e o repositório.
/// - **Repository Pattern**: ICouponRepository abstrai o MongoDB.
/// - **DTOs**: CreateCouponRequest e CouponValidationResult desacoplam a API.
/// </summary>
public class CouponService
{
    private readonly ICouponRepository _repository;

    public CouponService(ICouponRepository repository) => _repository = repository;

    ///
    /// <summary>
    /// Cria um novo cupom com as regras definidas no request.
    /// </summary>
    public async Task<Coupon> CreateCouponAsync(CreateCouponRequest request, CancellationToken ct = default)
    {
        var coupon = new Coupon(
            request.Code, request.Description, request.Type, request.Value,
            request.MinPurchaseAmount, request.MaxUses, request.MaxUsesPerCustomer,
            request.ValidFrom, request.ValidTo, request.ApplicableCategoryId);

        await _repository.CreateAsync(coupon, ct);
        return coupon;
    }

    ///
    /// <summary>
    /// Busca um cupom pelo código (case-insensitive).
    /// </summary>
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _repository.GetByCodeAsync(code.ToUpperInvariant(), ct);
    }

    ///
    /// <summary>
    /// Valida um cupom para o valor da compra e aplica o desconto se válido.
    /// Incrementa o contador de usos do cupom. Retorna um <see cref="CouponValidationResult"/>
    /// indicando sucesso ou falha.
    /// </summary>
    public async Task<CouponValidationResult> ValidateAndApplyAsync(string code, decimal purchaseAmount, string? customerId = null, string? categoryId = null, CancellationToken ct = default)
    {
        var coupon = await _repository.GetByCodeAsync(code.ToUpperInvariant(), ct);
        if (coupon is null)
            return new CouponValidationResult { IsValid = false, Error = "Coupon not found" };

        if (!coupon.IsValidFor(purchaseAmount, customerId, categoryId))
            return new CouponValidationResult { IsValid = false, Error = "Coupon is not applicable" };

        var discount = coupon.Apply(purchaseAmount);
        coupon.Use();
        await _repository.UpdateAsync(coupon, ct);

        return new CouponValidationResult
        {
            IsValid = true,
            DiscountAmount = discount,
            CouponCode = coupon.Code
        };
    }

    ///
    /// <summary>
    /// Desativa um cupom pelo código (impede novos usos).
    /// </summary>
    public async Task<Coupon?> DeactivateAsync(string code, CancellationToken ct = default)
    {
        var coupon = await _repository.GetByCodeAsync(code.ToUpperInvariant(), ct);
        if (coupon is null) return null;
        coupon.Deactivate();
        await _repository.UpdateAsync(coupon, ct);
        return coupon;
    }
}

///
/// <summary>
/// DTO de entrada para criação de um cupom.
/// </summary>
public class CreateCouponRequest
{
    public string Code { get; init; } = "";
    public string Description { get; init; } = "";
    public DiscountType Type { get; init; }
    public decimal Value { get; init; }
    public decimal? MinPurchaseAmount { get; init; }
    public int? MaxUses { get; init; }
    public int? MaxUsesPerCustomer { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public string? ApplicableCategoryId { get; init; }
}

///
/// <summary>
/// DTO de resultado da validação/aplicação de um cupom.
/// </summary>
public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
}

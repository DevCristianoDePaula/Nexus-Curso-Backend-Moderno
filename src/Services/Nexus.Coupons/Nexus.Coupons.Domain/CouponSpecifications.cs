using Nexus.Shared.Kernel.Specifications;

namespace Nexus.Coupons.Domain;

/// <summary>
/// Specification (padrão DDD) que verifica se o cupom está ativo.
/// Cupons inativos não devem ser aplicados independentemente de outras condições.
/// </summary>
public sealed class ActiveCouponSpecification : Specification<Coupon>
{
    public ActiveCouponSpecification() : base(c => c.IsActive) { }
}

/// <summary>
/// Specification que verifica se o cupom está dentro do período de validade.
/// Considera data de início (ValidFrom) e data de fim (ValidTo).
/// Se uma das datas não foi definida, aquela condição é ignorada.
/// </summary>
public sealed class ValidDateCouponSpecification : Specification<Coupon>
{
    public ValidDateCouponSpecification()
        : base(c => (!c.ValidFrom.HasValue || c.ValidFrom.Value <= DateTime.UtcNow)
                 && (!c.ValidTo.HasValue || c.ValidTo.Value >= DateTime.UtcNow))
    { }
}

/// <summary>
/// Specification que verifica se o cupom ainda tem usos disponíveis.
/// Compara o limite máximo (MaxUses) com o contador atual (CurrentUses).
/// Se MaxUses não foi definido, o cupom tem usos ilimitados.
/// </summary>
public sealed class AvailableUsesSpecification : Specification<Coupon>
{
    public AvailableUsesSpecification()
        : base(c => !c.MaxUses.HasValue || c.CurrentUses < c.MaxUses.Value)
    { }
}

/// <summary>
/// Specification que verifica se o valor da compra atende ao valor mínimo
/// exigido pelo cupom (MinPurchaseAmount).
/// O valor da compra é passado como parâmetro no construtor.
/// </summary>
public sealed class MinimumPurchaseSpecification : Specification<Coupon>
{
    private readonly decimal _purchaseAmount;

    public MinimumPurchaseSpecification(decimal purchaseAmount)
        : base(c => !c.MinPurchaseAmount.HasValue || purchaseAmount >= c.MinPurchaseAmount.Value)
    {
        _purchaseAmount = purchaseAmount;
    }
}

/// <summary>
/// Specification que verifica se o cupom é aplicável à categoria do produto.
/// Se ApplicableCategoryId for null, o cupom vale para qualquer categoria.
/// Caso contrário, a categoria do produto deve coincidir exatamente.
/// </summary>
public sealed class ApplicableCategorySpecification : Specification<Coupon>
{
    public ApplicableCategorySpecification(string? categoryId)
        : base(c => c.ApplicableCategoryId == null || c.ApplicableCategoryId == categoryId)
    { }
}

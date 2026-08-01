using Nexus.Coupons.Domain;

namespace Nexus.Coupons.Application;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task CreateAsync(Coupon coupon, CancellationToken ct = default);
    Task UpdateAsync(Coupon coupon, CancellationToken ct = default);
}

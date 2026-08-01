using FluentAssertions;
using Nexus.Coupons.Domain;

namespace Nexus.Tests.Domain;

public class CouponTests
{
    [Fact]
    public void Should_create_percentage_coupon()
    {
        var coupon = new Coupon("PROMO10", "10% off", DiscountType.Percentage, 10);

        coupon.Code.Should().Be("PROMO10");
        coupon.Description.Should().Be("10% off");
        coupon.Type.Should().Be(DiscountType.Percentage);
        coupon.Value.Should().Be(10);
        coupon.IsActive.Should().BeTrue();
        coupon.CurrentUses.Should().Be(0);
    }

    [Fact]
    public void Should_upper_case_code()
    {
        var coupon = new Coupon("promo10", "Desc", DiscountType.Fixed, 50);
        coupon.Code.Should().Be("PROMO10");
    }

    [Fact]
    public void Should_throw_on_non_positive_value()
    {
        var act = () => new Coupon("ZERO", "Desc", DiscountType.Fixed, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_apply_percentage_discount()
    {
        var coupon = new Coupon("P10", "10%", DiscountType.Percentage, 10);
        var discount = coupon.Apply(1000);

        discount.Should().Be(100);
    }

    [Fact]
    public void Should_apply_fixed_discount()
    {
        var coupon = new Coupon("FIXO50", "R$50 off", DiscountType.Fixed, 50);
        var discount = coupon.Apply(200);

        discount.Should().Be(50);
    }

    [Fact]
    public void Should_not_discount_more_than_purchase_amount()
    {
        var coupon = new Coupon("FIXO200", "R$200 off", DiscountType.Fixed, 200);
        var discount = coupon.Apply(50);

        discount.Should().Be(50);
    }

    [Fact]
    public void Should_use_coupon()
    {
        var coupon = new Coupon("USE", "Desc", DiscountType.Fixed, 10, maxUses: 5);
        coupon.Use();

        coupon.CurrentUses.Should().Be(1);
    }

    [Fact]
    public void Should_deactivate_and_activate()
    {
        var coupon = new Coupon("TEST", "Desc", DiscountType.Fixed, 10);
        coupon.Deactivate();
        coupon.IsActive.Should().BeFalse();

        coupon.Activate();
        coupon.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Should_be_invalid_when_inactive()
    {
        var coupon = new Coupon("INACTIVE", "Desc", DiscountType.Fixed, 10);
        coupon.Deactivate();

        coupon.IsValidFor(100).Should().BeFalse();
    }

    [Fact]
    public void Should_be_invalid_when_expired()
    {
        var coupon = new Coupon("EXP", "Desc", DiscountType.Fixed, 10,
            validTo: DateTime.UtcNow.AddDays(-1));

        coupon.IsValidFor(100).Should().BeFalse();
    }

    [Fact]
    public void Should_be_invalid_when_not_yet_valid()
    {
        var coupon = new Coupon("FUT", "Desc", DiscountType.Fixed, 10,
            validFrom: DateTime.UtcNow.AddDays(1));

        coupon.IsValidFor(100).Should().BeFalse();
    }

    [Fact]
    public void Should_be_invalid_when_max_uses_exceeded()
    {
        var coupon = new Coupon("MAX", "Desc", DiscountType.Fixed, 10, maxUses: 2);
        coupon.Use();
        coupon.Use();

        coupon.IsValidFor(100).Should().BeFalse();
    }

    [Fact]
    public void Should_be_invalid_when_below_min_purchase()
    {
        var coupon = new Coupon("MIN", "Desc", DiscountType.Fixed, 10,
            minPurchaseAmount: 500);

        coupon.IsValidFor(100).Should().BeFalse();
        coupon.IsValidFor(500).Should().BeTrue();
    }

    [Fact]
    public void Should_be_invalid_for_different_category()
    {
        var coupon = new Coupon("CAT", "Desc", DiscountType.Fixed, 10,
            applicableCategoryId: "cat-eletronicos");

        coupon.IsValidFor(100, categoryId: "cat-livros").Should().BeFalse();
        coupon.IsValidFor(100, categoryId: "cat-eletronicos").Should().BeTrue();
    }

    [Fact]
    public void Should_throw_on_apply_when_invalid()
    {
        var coupon = new Coupon("INV", "Desc", DiscountType.Fixed, 10);
        coupon.Deactivate();

        var act = () => coupon.Apply(100);
        act.Should().Throw<InvalidOperationException>();
    }
}

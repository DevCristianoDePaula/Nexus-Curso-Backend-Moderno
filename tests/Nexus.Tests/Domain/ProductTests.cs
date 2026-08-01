using FluentAssertions;
using Nexus.Catalog.Domain;

namespace Nexus.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Should_create_valid_product()
    {
        var product = new Product(
            "Smartphone XYZ",
            "Smartphone de última geração",
            new Money(2999.99m, "BRL"),
            "cat-1", "Eletrônicos",
            "sel-1", "Loja Tech",
            new Sku("SMART-001"),
            50);

        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be("Smartphone XYZ");
        product.Price.Amount.Should().Be(2999.99m);
        product.Sku.Value.Should().Be("SMART-001");
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void Should_throw_on_empty_name()
    {
        var act = () => new Product("", "Desc", Money.Zero(), "cat-1", "Cat", "sel-1", "Seller", new Sku("SKU-001"), 10);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_throw_on_negative_price()
    {
        var act = () => new Money(-10, "BRL");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_decrease_stock()
    {
        var product = new Product("Test", "Desc", Money.Zero(), "cat-1", "Cat", "sel-1", "Seller", new Sku("SKU-001"), 10);
        product.DecreaseStock(3);
        product.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void Should_throw_on_insufficient_stock()
    {
        var product = new Product("Test", "Desc", Money.Zero(), "cat-1", "Cat", "sel-1", "Seller", new Sku("SKU-001"), 2);
        var act = () => product.DecreaseStock(5);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_deactivate_and_reactivate()
    {
        var product = new Product("Test", "Desc", Money.Zero(), "cat-1", "Cat", "sel-1", "Seller", new Sku("SKU-001"), 10);
        product.Deactivate();
        product.Status.Should().Be(ProductStatus.Inactive);
        product.Activate();
        product.Status.Should().Be(ProductStatus.Active);
    }
}

public class SkuTests
{
    [Fact]
    public void Should_validate_sku_format()
    {
        var act = () => new Sku("sku com espaços");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_uppercase_sku()
    {
        var sku = new Sku("abc-123");
        sku.Value.Should().Be("ABC-123");
    }
}

public class MoneyTests
{
    [Fact]
    public void Should_create_valid_money()
    {
        var money = new Money(100.50m, "BRL");
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Should_uppercase_currency()
    {
        var money = new Money(10, "brl");
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Should_implicitly_convert_to_decimal()
    {
        decimal amount = new Money(99.90m, "BRL");
        amount.Should().Be(99.90m);
    }
}
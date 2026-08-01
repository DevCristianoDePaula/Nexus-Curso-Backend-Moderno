using FluentAssertions;
using CartDomain = Nexus.Cart.Domain;

namespace Nexus.Tests.Domain;

public class CartTests
{
    [Fact]
    public void Should_create_empty_cart()
    {
        var cart = new CartDomain.Cart("user-1");

        cart.UserId.Should().Be("user-1");
        cart.Items.Should().BeEmpty();
        cart.Total.Should().Be(0);
    }

    [Fact]
    public void Should_add_item()
    {
        var cart = new CartDomain.Cart("user-1");
        cart.AddItem("prod-1", "Smartphone", 1999.99m, "BRL", 2);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be("prod-1");
        cart.Items[0].Quantity.Should().Be(2);
        cart.Items[0].Subtotal.Should().Be(3999.98m);
        cart.Total.Should().Be(3999.98m);
    }

    [Fact]
    public void Should_increment_quantity_when_adding_existing_item()
    {
        var cart = new CartDomain.Cart("user-1");
        cart.AddItem("prod-1", "Smartphone", 1000, "BRL", 1);
        cart.AddItem("prod-1", "Smartphone", 1000, "BRL", 3);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(4);
        cart.Total.Should().Be(4000);
    }

    [Fact]
    public void Should_remove_item()
    {
        var cart = new CartDomain.Cart("user-1");
        cart.AddItem("prod-1", "Item", 100, "BRL");
        cart.AddItem("prod-2", "Item 2", 200, "BRL");
        cart.RemoveItem("prod-1");

        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be("prod-2");
    }

    [Fact]
    public void Should_update_quantity()
    {
        var cart = new CartDomain.Cart("user-1");
        cart.AddItem("prod-1", "Item", 100, "BRL", 2);
        cart.UpdateQuantity("prod-1", 5);

        cart.Items[0].Quantity.Should().Be(5);
        cart.Total.Should().Be(500);
    }

    [Fact]
    public void Should_remove_item_when_quantity_is_zero_or_negative()
    {
        var cart = new CartDomain.Cart("user-1");
        cart.AddItem("prod-1", "Item", 100, "BRL");
        cart.UpdateQuantity("prod-1", 0);

        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Should_clear_cart()
    {
        var cart = new CartDomain.Cart("user-1");
        cart.AddItem("prod-1", "Item", 100, "BRL");
        cart.AddItem("prod-2", "Item 2", 200, "BRL");
        cart.Clear();

        cart.Items.Should().BeEmpty();
        cart.Total.Should().Be(0);
    }

    [Fact]
    public void Should_calculate_total_with_multiple_items()
    {
        var cart = new CartDomain.Cart("user-1");
        cart.AddItem("prod-1", "Item A", 50, "BRL", 2);
        cart.AddItem("prod-2", "Item B", 30, "BRL", 3);
        cart.AddItem("prod-3", "Item C", 20, "BRL", 1);

        cart.Total.Should().Be(210);
    }
}

public class CartItemTests
{
    [Fact]
    public void Should_create_cart_item()
    {
        var item = new CartDomain.CartItem("prod-1", "Product", 99.90m, "BRL", 2);

        item.ProductId.Should().Be("prod-1");
        item.ProductName.Should().Be("Product");
        item.UnitPrice.Should().Be(99.90m);
        item.Currency.Should().Be("BRL");
        item.Quantity.Should().Be(2);
        item.Subtotal.Should().Be(199.80m);
    }

    [Fact]
    public void Should_throw_on_negative_price()
    {
        var act = () => new CartDomain.CartItem("p1", "P", -1, "BRL", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_throw_on_zero_quantity()
    {
        var act = () => new CartDomain.CartItem("p1", "P", 10, "BRL", 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_add_quantity()
    {
        var item = new CartDomain.CartItem("p1", "P", 10, "BRL", 1);
        item.AddQuantity(3);
        item.Quantity.Should().Be(4);
    }

    [Fact]
    public void Should_set_quantity()
    {
        var item = new CartDomain.CartItem("p1", "P", 10, "BRL", 1);
        item.SetQuantity(10);
        item.Quantity.Should().Be(10);
    }

    [Fact]
    public void Should_throw_on_set_quantity_zero()
    {
        var item = new CartDomain.CartItem("p1", "P", 10, "BRL", 1);
        var act = () => item.SetQuantity(0);
        act.Should().Throw<ArgumentException>();
    }
}

using FluentAssertions;
using Nexus.Orders.Domain;

namespace Nexus.Tests.Domain;

public class OrderTests
{
    private readonly Address _address = new("Rua A", "123", "Centro", "São Paulo", "SP", "01001-000");

    [Fact]
    public void Should_create_pending_order()
    {
        var order = new Order("customer-1", _address);

        order.CustomerId.Should().Be("customer-1");
        order.ShippingAddress.Should().Be(_address);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
        order.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Should_add_item_to_pending_order()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Smartphone", 1999.99m, 2);

        order.Items.Should().HaveCount(1);
        order.Items[0].ProductId.Should().Be("prod-1");
        order.Items[0].Quantity.Should().Be(2);
        order.TotalAmount.Should().Be(3999.98m);
    }

    [Fact]
    public void Should_throw_on_add_item_to_non_pending_order()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 100, 1);
        order.Submit();

        var act = () => order.AddItem("prod-2", "Another", 50, 1);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_apply_coupon()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 1000, 1);
        order.ApplyCoupon("PROMO10", 100);

        order.CouponCode.Should().Be("PROMO10");
        order.DiscountAmount.Should().Be(100);
        order.TotalAmount.Should().Be(900);
    }

    [Fact]
    public void Should_throw_on_invalid_coupon_discount()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 100, 1);

        var act = () => order.ApplyCoupon("INVALID", 200);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_submit_order()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 100, 1);
        order.Submit();

        order.Status.Should().Be(OrderStatus.Submitted);
    }

    [Fact]
    public void Should_throw_on_submit_empty_order()
    {
        var order = new Order("customer-1", _address);
        var act = () => order.Submit();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_confirm_payment()
    {
        var order = CreateSubmittedOrder();
        order.ConfirmPayment("pay-123");

        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentId.Should().Be("pay-123");
    }

    [Fact]
    public void Should_throw_on_confirm_payment_before_submit()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 100, 1);

        var act = () => order.ConfirmPayment("pay-123");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_ship_order()
    {
        var order = CreatePaidOrder();
        order.Ship();

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Should_throw_on_ship_unpaid_order()
    {
        var order = CreateSubmittedOrder();
        var act = () => order.Ship();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_deliver_order()
    {
        var order = CreatePaidOrder();
        order.Ship();
        order.Deliver();

        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Should_throw_on_deliver_before_ship()
    {
        var order = CreatePaidOrder();
        var act = () => order.Deliver();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_cancel_pending_order()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 100, 1);
        order.Cancel("Mudou de ideia");

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Should_throw_on_cancel_delivered_order()
    {
        var order = CreatePaidOrder();
        order.Ship();
        order.Deliver();

        var act = () => order.Cancel("Motivo");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_submit_raise_domain_event()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 100, 1);
        order.Submit();

        order.DomainEvents.Should().ContainSingle(e => e is OrderSubmittedEvent);
    }

    [Fact]
    public void Should_confirm_payment_raise_domain_event()
    {
        var order = CreateSubmittedOrder();
        order.ConfirmPayment("pay-123");

        order.DomainEvents.Should().Contain(e => e is OrderPaidEvent);
    }

    private Order CreateSubmittedOrder()
    {
        var order = new Order("customer-1", _address);
        order.AddItem("prod-1", "Item", 100, 1);
        order.Submit();
        return order;
    }

    private Order CreatePaidOrder()
    {
        var order = CreateSubmittedOrder();
        order.ConfirmPayment("pay-123");
        return order;
    }
}

public class OrderItemTests
{
    [Fact]
    public void Should_create_order_item()
    {
        var item = new OrderItem("prod-1", "Product", 99.90m, "BRL", 2);

        item.ProductId.Should().Be("prod-1");
        item.ProductName.Should().Be("Product");
        item.UnitPrice.Should().Be(99.90m);
        item.Quantity.Should().Be(2);
        item.Subtotal.Should().Be(199.80m);
    }

    [Fact]
    public void Should_throw_on_negative_price()
    {
        var act = () => new OrderItem("p1", "P", -5, "BRL", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_throw_on_zero_quantity()
    {
        var act = () => new OrderItem("p1", "P", 10, "BRL", 0);
        act.Should().Throw<ArgumentException>();
    }
}

public class AddressTests
{
    [Fact]
    public void Should_create_address()
    {
        var addr = new Address("Rua A", "123", "Centro", "São Paulo", "SP", "01001-000", "Apto 42");

        addr.Street.Should().Be("Rua A");
        addr.Number.Should().Be("123");
        addr.Complement.Should().Be("Apto 42");
        addr.City.Should().Be("São Paulo");
        addr.State.Should().Be("SP");
        addr.ZipCode.Should().Be("01001-000");
    }

    [Fact]
    public void Should_throw_on_null_street()
    {
        var act = () => new Address(null!, "123", "Centro", "SP", "SP", "00000-000");
        act.Should().Throw<ArgumentNullException>();
    }
}

using FluentAssertions;
using Nexus.Payments.Domain;

namespace Nexus.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void Should_create_pending_payment()
    {
        var payment = new Payment(Guid.NewGuid(), 299.90m, "BRL", PaymentMethod.Pix);

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Should().Be(299.90m);
        payment.Currency.Should().Be("BRL");
        payment.Method.Should().Be(PaymentMethod.Pix);
    }

    [Fact]
    public void Should_throw_on_non_positive_amount()
    {
        var act = () => new Payment(Guid.NewGuid(), 0, "BRL", PaymentMethod.CreditCard);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_approve_payment()
    {
        var payment = new Payment(Guid.NewGuid(), 100, "BRL", PaymentMethod.Boleto);
        payment.Approve("txn-123");

        payment.Status.Should().Be(PaymentStatus.Approved);
        payment.TransactionId.Should().Be("txn-123");
    }

    [Fact]
    public void Should_raise_event_when_approved()
    {
        var orderId = Guid.NewGuid();
        var payment = new Payment(orderId, 100, "BRL", PaymentMethod.CreditCard);
        payment.Approve("txn-abc");

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentApprovedEvent);
        var evt = payment.DomainEvents.OfType<PaymentApprovedEvent>().Single();
        evt.OrderId.Should().Be(orderId);
        evt.TransactionId.Should().Be("txn-abc");
    }

    [Fact]
    public void Should_decline_payment()
    {
        var payment = new Payment(Guid.NewGuid(), 100, "BRL", PaymentMethod.CreditCard);
        payment.Decline("Saldo insuficiente");

        payment.Status.Should().Be(PaymentStatus.Declined);
        payment.FailureReason.Should().Be("Saldo insuficiente");
    }

    [Fact]
    public void Should_raise_event_when_declined()
    {
        var orderId = Guid.NewGuid();
        var payment = new Payment(orderId, 100, "BRL", PaymentMethod.CreditCard);
        payment.Decline("Recusado");

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentDeclinedEvent);
        var evt = payment.DomainEvents.OfType<PaymentDeclinedEvent>().Single();
        evt.OrderId.Should().Be(orderId);
        evt.Reason.Should().Be("Recusado");
    }

    [Fact]
    public void Should_refund_approved_payment()
    {
        var payment = new Payment(Guid.NewGuid(), 100, "BRL", PaymentMethod.Pix);
        payment.Approve("txn-123");
        payment.Refund();

        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Should_throw_on_refund_non_approved_payment()
    {
        var payment = new Payment(Guid.NewGuid(), 100, "BRL", PaymentMethod.Pix);
        var act = () => payment.Refund();
        act.Should().Throw<InvalidOperationException>();
    }
}

using FluentAssertions;
using Nexus.Users.Domain;

namespace Nexus.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Should_create_customer_user()
    {
        var user = new NexusUser("customer@test.com", "João Silva", UserType.Customer, "123.456.789-00");

        user.Email.Should().Be("customer@test.com");
        user.UserName.Should().Be("customer@test.com");
        user.FullName.Should().Be("João Silva");
        user.Type.Should().Be(UserType.Customer);
        user.Cpf.Should().Be("123.456.789-00");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Should_create_seller_user_without_cpf()
    {
        var user = new NexusUser("seller@store.com", "Maria Souza", UserType.Seller);

        user.Type.Should().Be(UserType.Seller);
        user.Cpf.Should().BeNull();
    }

    [Fact]
    public void Should_update_profile()
    {
        var user = new NexusUser("user@test.com", "Old Name", UserType.Customer);
        user.UpdateProfile("New Name", "111.222.333-44");

        user.FullName.Should().Be("New Name");
        user.Cpf.Should().Be("111.222.333-44");
    }

    [Fact]
    public void Should_promote_to_admin()
    {
        var user = new NexusUser("user@test.com", "User", UserType.Customer);
        user.PromoteToAdmin();

        user.Type.Should().Be(UserType.Admin);
    }
}

public class StoreTests
{
    [Fact]
    public void Should_create_store()
    {
        var store = new Store("Tech Store", "Loja de tecnologia", "seller-1");

        store.Name.Should().Be("Tech Store");
        store.Description.Should().Be("Loja de tecnologia");
        store.SellerId.Should().Be("seller-1");
        store.Rating.Should().Be(0);
        store.RatingCount.Should().Be(0);
    }

    [Fact]
    public void Should_throw_on_null_name()
    {
        var act = () => new Store(null!, "Desc", "seller-1");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_update_profile()
    {
        var store = new Store("Old", "Old desc", "seller-1");
        store.UpdateProfile("New Store", "New desc", "http://logo.url");

        store.Name.Should().Be("New Store");
        store.Description.Should().Be("New desc");
        store.LogoUrl.Should().Be("http://logo.url");
    }

    [Fact]
    public void Should_update_rating()
    {
        var store = new Store("Store", "Desc", "seller-1");
        store.UpdateRating(4.5);

        store.Rating.Should().Be(4.5);
        store.RatingCount.Should().Be(1);

        store.UpdateRating(5.0);
        store.Rating.Should().Be(4.75);
        store.RatingCount.Should().Be(2);
    }
}

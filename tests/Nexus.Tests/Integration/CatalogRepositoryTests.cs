using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nexus.Catalog.Application;
using Nexus.Catalog.Domain;
using Nexus.Catalog.Infrastructure;
using Testcontainers.MongoDb;

namespace Nexus.Tests.Integration;

public class CatalogRepositoryTests : IAsyncLifetime
{
    private static bool _guidSerializerRegistered;

    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithImage("mongo:8")
        .Build();

    private ICatalogRepository _repository = null!;
    private ICategoryRepository _categoryRepo = null!;

    public async Task InitializeAsync()
    {
        await _mongo.StartAsync();
        if (!_guidSerializerRegistered)
        {
            BsonSerializer.TryRegisterSerializer(typeof(Guid), new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
            _guidSerializerRegistered = true;
        }
        var client = new MongoClient(_mongo.GetConnectionString());
        var db = client.GetDatabase("test_catalog");
        _repository = new CatalogRepository(db);
        _categoryRepo = new CategoryRepository(db);
    }

    public async Task DisposeAsync()
    {
        await _mongo.DisposeAsync();
    }

    [Fact]
    public async Task Should_create_and_retrieve_product()
    {
        var category = new Category("Eletrônicos");
        await _categoryRepo.CreateAsync(category);

        var product = new Product(
            "Smartphone XYZ",
            "Smartphone de última geração",
            new Money(2999.99m, "BRL"),
            category.Id, category.Name,
            "sel-1", "Tech Store",
            new Sku("SMART-001"),
            50);

        await _repository.CreateAsync(product);

        var retrieved = await _repository.GetByIdAsync(product.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Smartphone XYZ");
        retrieved.Price.Amount.Should().Be(2999.99m);
    }

    [Fact]
    public async Task Should_search_products()
    {
        var category = new Category("Eletrônicos");
        await _categoryRepo.CreateAsync(category);

        var p1 = new Product("Smartphone ABC", "Smartphone", new Money(1000, "BRL"), category.Id, category.Name, "sel-1", "Loja", new Sku("PHONE-001"), 10);
        var p2 = new Product("Tablet XYZ", "Tablet", new Money(2000, "BRL"), category.Id, category.Name, "sel-1", "Loja", new Sku("TAB-001"), 10);
        await _repository.CreateAsync(p1);
        await _repository.CreateAsync(p2);

        var results = await _repository.SearchAsync("smartphone", 1, 20);
        results.Should().ContainSingle();
        results[0].Name.Should().Be("Smartphone ABC");
    }

    [Fact]
    public async Task Should_return_empty_when_no_match()
    {
        var results = await _repository.SearchAsync("nonexistent-product-xyz", 1, 20);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_update_product()
    {
        var category = new Category("Test");
        await _categoryRepo.CreateAsync(category);

        var product = new Product("Original", "Desc", new Money(100, "BRL"), category.Id, category.Name, "sel-1", "Loja", new Sku("ORIG-001"), 10);
        await _repository.CreateAsync(product);

        product.SetPrice(new Money(150, "BRL"));
        await _repository.UpdateAsync(product);

        var updated = await _repository.GetByIdAsync(product.Id);
        updated!.Price.Amount.Should().Be(150);
    }
}
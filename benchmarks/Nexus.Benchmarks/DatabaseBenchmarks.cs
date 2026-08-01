using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Nexus.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(iterationCount: 10, warmupCount: 3)]
public class DatabaseBenchmarks
{
    private const string SqlConnectionString = "Server=localhost;Database=Nexus_Benchmarks;User Id=sa;Password=Nexus@2026#;TrustServerCertificate=True;";
    private const string MongoConnectionString = "mongodb://nexus:Nexus@2026#@localhost:27017";
    private SqlConnection _sqlConnection;
    private BenchmarksDbContext _efContext;
    private IMongoCollection<BenchmarkProduct> _mongoCollection;

    [GlobalSetup]
    public void Setup()
    {
        _sqlConnection = new SqlConnection(SqlConnectionString);
        _efContext = new BenchmarksDbContext(
            new DbContextOptionsBuilder<BenchmarksDbContext>()
                .UseSqlServer(SqlConnectionString)
                .Options);

        var mongoClient = new MongoClient(MongoConnectionString);
        var mongoDb = mongoClient.GetDatabase("nexus_benchmarks");
        _mongoCollection = mongoDb.GetCollection<BenchmarkProduct>("products");

        EnsureData();
    }

    private void EnsureData()
    {
        _sqlConnection.Execute("""
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BenchmarkProducts' AND xtype='U')
            CREATE TABLE BenchmarkProducts (
                Id INT IDENTITY PRIMARY KEY,
                Name NVARCHAR(200),
                Description NVARCHAR(2000),
                Price DECIMAL(18,2),
                Category NVARCHAR(100),
                StockQuantity INT
            )
        """);
    }

    [Benchmark(Baseline = true)]
    public List<BenchmarkProduct> EfCore_ReadAll()
    {
        return _efContext.Products.ToList();
    }

    [Benchmark]
    public List<BenchmarkProduct> EfCore_ReadFiltered()
    {
        return _efContext.Products.Where(p => p.Price > 50).ToList();
    }

    [Benchmark]
    public List<BenchmarkProduct> Dapper_ReadAll()
    {
        return _sqlConnection.Query<BenchmarkProduct>("SELECT * FROM BenchmarkProducts").ToList();
    }

    [Benchmark]
    public List<BenchmarkProduct> Dapper_ReadFiltered()
    {
        return _sqlConnection.Query<BenchmarkProduct>("SELECT * FROM BenchmarkProducts WHERE Price > 50").ToList();
    }

    [Benchmark]
    public List<BenchmarkProduct> Dapper_RawSql()
    {
        return _sqlConnection.Query<BenchmarkProduct>("SELECT Id, Name, Description, Price, Category, StockQuantity FROM BenchmarkProducts WHERE Price > @MinPrice", new { MinPrice = 50 }).ToList();
    }

    [Benchmark]
    public List<BenchmarkProduct> MongoDb_ReadAll()
    {
        return _mongoCollection.Find(FilterDefinition<BenchmarkProduct>.Empty).ToList();
    }

    [Benchmark]
    public List<BenchmarkProduct> MongoDb_ReadFiltered()
    {
        return _mongoCollection.Find(p => p.Price > 50).ToList();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _sqlConnection?.Dispose();
        _efContext?.Dispose();
    }
}

public class BenchmarkProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string Category { get; set; } = "";
    public int StockQuantity { get; set; }
}

public class BenchmarksDbContext : DbContext
{
    public DbSet<BenchmarkProduct> Products => Set<BenchmarkProduct>();
    public BenchmarksDbContext(DbContextOptions<BenchmarksDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<BenchmarkProduct>(e =>
        {
            e.ToTable("BenchmarkProducts");
            e.HasKey(p => p.Id);
        });
    }
}

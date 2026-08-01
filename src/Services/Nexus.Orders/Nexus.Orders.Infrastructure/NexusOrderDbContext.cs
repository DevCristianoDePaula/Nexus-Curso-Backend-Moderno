using Microsoft.EntityFrameworkCore;
using Nexus.Orders.Domain;

namespace Nexus.Orders.Infrastructure;

///
/// <summary>
/// Contexto do Entity Framework Core para o módulo de Pedidos (SQL Server).
/// 
/// Padrão **DbContext**: unidade de trabalho que gerencia as entidades de domínio
/// e configura o mapeamento objeto-relacional via Fluent API.
/// 
/// DbSets:
/// - Orders: entidade raiz (Aggregate Root) do módulo de pedidos.
/// 
/// Configurações:
/// - OwnsOne: ShippingAddress é um Value Object embutido na mesma tabela.
/// - OwnsMany: Items é uma collection de Value Objects em tabela separada.
/// - Ignore: DomainEvents não é mapeado (eventos são transient, não persistem).
/// </summary>
public class NexusOrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public NexusOrderDbContext(DbContextOptions<NexusOrderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configuração Fluent API para a entidade Order (Aggregate Root).
        builder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);

            // Colunas simples com tamanho máximo.
            e.Property(o => o.CustomerId).HasMaxLength(450);
            e.Property(o => o.Currency).HasMaxLength(3);
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(o => o.CouponCode).HasMaxLength(50);

            // Value Object ShippingAddress — mapeado como colunas na mesma tabela (OwnsOne).
            e.OwnsOne(o => o.ShippingAddress, a =>
            {
                a.Property(p => p.Street).HasMaxLength(200);
                a.Property(p => p.Number).HasMaxLength(20);
                a.Property(p => p.Complement).HasMaxLength(200);
                a.Property(p => p.Neighborhood).HasMaxLength(100);
                a.Property(p => p.City).HasMaxLength(100);
                a.Property(p => p.State).HasMaxLength(50);
                a.Property(p => p.ZipCode).HasMaxLength(20);
            });

            // Collection de Value Objects OrderItem — mapeada em tabela separada (OwnsMany).
            e.OwnsMany(o => o.Items, item =>
            {
                item.WithOwner().HasForeignKey("OrderId");
                item.HasKey(i => i.Id);
                item.Property(i => i.ProductId).HasMaxLength(450);
                item.Property(i => i.ProductName).HasMaxLength(200);
                item.Property(i => i.Currency).HasMaxLength(3);
            });

            // DomainEvents são transient — não devem ser persistidos no banco.
            e.Ignore(o => o.DomainEvents);
        });
    }
}

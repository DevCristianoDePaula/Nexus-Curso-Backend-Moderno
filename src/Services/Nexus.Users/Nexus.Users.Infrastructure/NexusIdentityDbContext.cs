using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nexus.Users.Domain;

namespace Nexus.Users.Infrastructure;

///
/// <summary>
/// Contexto do Entity Framework Core para o módulo de Usuários.
/// Herda de <see cref="IdentityDbContext{NexusUser}"/> para integrar o ASP.NET Core Identity
/// com o SQL Server, aproveitando todas as tabelas de autenticação (AspNetUsers, Roles, etc.).
/// 
/// Padrão **DbContext**: unidade de trabalho do EF Core que gerencia as entidades
/// e o mapeamento objeto-relacional (ORM).
/// 
/// DbSets:
/// - Stores: entidade de domínio que representa uma loja vinculada a um Seller.
/// </summary>
public class NexusIdentityDbContext : IdentityDbContext<NexusUser>
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public NexusIdentityDbContext(DbContextOptions<NexusIdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Store>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).HasMaxLength(200);
            e.Property(s => s.Description).HasMaxLength(2000);
            e.Property(s => s.SellerId).HasMaxLength(450);
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Token).IsUnique();
            e.HasIndex(t => t.UserId);
            e.Property(t => t.UserId).HasMaxLength(450);
        });
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Users.Application;
using Nexus.Users.Domain;

namespace Nexus.Users.Infrastructure;

///
/// <summary>
/// Classe estática de registro de dependências do módulo Users.Infrastructure.
/// 
/// Padrão **ServiceRegistration**: centraliza a configuração do Identity, EF Core e
/// serviços de aplicação em um único método de extensão.
/// 
/// Registros:
/// - NexusIdentityDbContext com SQL Server
/// - ASP.NET Core Identity com EntityFramework stores
/// - AuthService (Application Service)
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDb")
            ?? "Server=localhost;Database=Nexus_Users;User Id=sa;Password=Nexus@2026#;TrustServerCertificate=True";

        // Configura o DbContext com SQL Server.
        services.AddDbContext<NexusIdentityDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Configura o ASP.NET Core Identity com regras customizadas de senha.
        services.AddIdentity<NexusUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<NexusIdentityDbContext>() // Armazena usuários/roles no SQL Server.
        .AddDefaultTokenProviders(); // Providers para tokens (e-mail, reset de senha, etc.).

        services.AddScoped<AuthService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}

using Microsoft.AspNetCore.Identity;

namespace Nexus.Users.Domain;

/// <summary>
/// Agregado raiz do domínio de Usuários. Representa uma conta de usuário
/// na plataforma. Herda de IdentityUser para integração com o ASP.NET Core Identity,
/// adicionando propriedades específicas do negócio como nome completo, CPF e tipo.
/// No DDD, esta entidade gerencia o ciclo de vida do usuário e suas permissões.
/// </summary>
public class NexusUser : IdentityUser
{
    // Nome completo do usuário (obrigatório)
    public string FullName { get; private set; } = "";

    // CPF opcional — usado apenas para clientes pessoa física (Customer)
    public string? Cpf { get; private set; }

    // Tipo de usuário: Customer, Seller ou Admin (controla permissões)
    public UserType Type { get; private set; }

    // Data de registro do usuário (imutável)
    public DateTime CreatedAt { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private NexusUser() { }

    /// <summary>
    /// Cria um novo usuário. O UserName é definido como o próprio email
    /// (padrão do Identity) e a data de criação é registrada como UTC.
    /// </summary>
    public NexusUser(string email, string fullName, UserType type, string? cpf = null)
    {
        Email = email;
        UserName = email;
        FullName = fullName;
        Type = type;
        Cpf = cpf;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza os dados de perfil do usuário (nome e CPF).
    /// </summary>
    public void UpdateProfile(string fullName, string? cpf)
    {
        FullName = fullName;
        Cpf = cpf;
    }

    /// <summary>
    /// Promove o usuário a administrador. Operação crítica — deve ser usada
    /// com cautela (normalmente via comando administrativo).
    /// </summary>
    public void PromoteToAdmin() => Type = UserType.Admin;
}

/// <summary>
/// Enum que define os tipos de usuário na plataforma.
/// Customer: comprador, Seller: vendedor (dono de loja), Admin: administrador geral.
/// </summary>
public enum UserType
{
    Customer,
    Seller,
    Admin
}

/// <summary>
/// Enum que define os tipos de documento fiscal suportados.
/// CPF para pessoa física, CNPJ para pessoa jurídica (vendedores).
/// </summary>
public enum UserDocumentType
{
    Cpf,
    Cnpj
}

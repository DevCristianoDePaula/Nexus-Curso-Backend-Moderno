using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nexus.Users.Domain;

namespace Nexus.Users.Application;

///
/// <summary>
/// Serviço de aplicação responsável por autenticação e registro de usuários.
/// Utiliza o ASP.NET Core Identity como mecanismo de gerenciamento de usuários e roles,
/// e gera tokens JWT para sessões autenticadas.
/// 
/// Padrões aplicados:
/// - **Application Service**: orquestra chamadas ao Identity e ao domínio.
/// - **DTO**: requests (RegisterRequest, LoginRequest) e result (AuthResult) desacoplam a camada de API.
/// </summary>
public class AuthService
{
    private readonly UserManager<NexusUser> _userManager;
    private readonly SignInManager<NexusUser> _signInManager;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<NexusUser> userManager,
        SignInManager<NexusUser> signInManager,
        IRefreshTokenRepository refreshTokenRepo,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _refreshTokenRepo = refreshTokenRepo;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var user = new NexusUser(request.Email, request.FullName, request.Type, request.Cpf);
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return new AuthResult { Succeeded = false, Errors = result.Errors.Select(e => e.Description) };

        await _userManager.AddToRoleAsync(user, request.Type.ToString());
        return new AuthResult { Succeeded = true };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new AuthResult { Succeeded = false, Errors = ["Invalid credentials"] };

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return new AuthResult { Succeeded = false, Errors = ["Invalid credentials"] };

        var token = await GenerateTokenAsync(user);
        var refreshToken = new RefreshToken(user.Id, TimeSpan.FromDays(7));
        await _refreshTokenRepo.AddAsync(refreshToken);

        return new AuthResult { Succeeded = true, Token = token, RefreshToken = refreshToken.Token };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
        if (stored is null || !stored.IsActive)
            return new AuthResult { Succeeded = false, Errors = ["Invalid or expired refresh token"] };

        stored.Revoke();
        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user is null)
            return new AuthResult { Succeeded = false, Errors = ["User not found"] };

        var newToken = await GenerateTokenAsync(user);
        var newRefreshToken = new RefreshToken(user.Id, TimeSpan.FromDays(7));
        await _refreshTokenRepo.AddAsync(newRefreshToken);

        return new AuthResult { Succeeded = true, Token = newToken, RefreshToken = newRefreshToken.Token };
    }

    public async Task<AuthResult> LogoutAsync(string userId)
    {
        await _refreshTokenRepo.RevokeAllForUserAsync(userId);
        return new AuthResult { Succeeded = true };
    }

    // Gera um token JWT contendo claims do usuário (ID, e-mail, nome, tipo, roles).
    // Utiliza HMAC-SHA256 com chave simétrica configurada em appsettings.json.
    private async Task<string> GenerateTokenAsync(NexusUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new("user_type", user.Type.ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "Nexus-Temporary-Dev-Key-Minimum-32-Characters!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Nexus",
            audience: _configuration["Jwt:Audience"] ?? "Nexus",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

///
/// <summary>
/// DTO que encapsula o resultado da operação de autenticação,
/// contendo indicador de sucesso, token JWT (se houver) e lista de erros.
/// </summary>
public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}

///
/// <summary>
/// DTO de entrada para o registro de um novo usuário.
/// </summary>
public class RegisterRequest
{
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
    public string FullName { get; init; } = "";
    public UserType Type { get; init; } = UserType.Customer;
    public string? Cpf { get; init; }
}

public class LoginRequest
{
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; init; } = "";
}

public class LogoutRequest
{
    public string UserId { get; init; } = "";
}

using Nexus.Shared.Kernel.Entities;

namespace Nexus.Users.Domain;

/// <summary>
/// Entidade que representa uma loja virtual na plataforma.
/// Cada loja pertence a um vendedor (Seller) e possui informações como nome,
/// descrição, logotipo e avaliação média dos clientes.
/// A avaliação utiliza média móvel para evitar recalcular todo o histórico.
/// </summary>
public class Store : Entity
{
    // Nome da loja (exibido na vitrine)
    public string Name { get; private set; }

    // Descrição da loja (usada na página da loja)
    public string Description { get; private set; }

    // ID do vendedor dono da loja (chave estrangeira para NexusUser)
    public string SellerId { get; private set; }

    // URL opcional do logotipo da loja
    public string? LogoUrl { get; private set; }

    // Avaliação média da loja (média ponderada das avaliações dos clientes)
    public double Rating { get; private set; }

    // Quantidade total de avaliações recebidas (usada no cálculo da média)
    public int RatingCount { get; private set; }

    // Construtor privado exigido pelo Entity Framework
    private Store() { }

    /// <summary>
    /// Cria uma nova loja vinculada a um vendedor. A loja começa sem avaliações.
    /// </summary>
    public Store(string name, string description, string sellerId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        SellerId = sellerId ?? throw new ArgumentNullException(nameof(sellerId));
    }

    /// <summary>
    /// Atualiza os dados de perfil público da loja.
    /// </summary>
    public void UpdateProfile(string name, string description, string? logoUrl)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        Touch();
    }

    /// <summary>
    /// Adiciona uma nova avaliação e recalcula a média usando média móvel.
    /// Fórmula: (média_atual * total_avaliacoes + nova_nota) / (total_avaliacoes + 1)
    /// Isso evita armazenar o histórico completo de avaliações.
    /// </summary>
    public void UpdateRating(double newRating)
    {
        Rating = ((Rating * RatingCount) + newRating) / (RatingCount + 1);
        RatingCount++;
        Touch();
    }
}

using System.Text.RegularExpressions;

namespace Nexus.Catalog.Domain;

/// <summary>
/// Value Object que representa um SKU (Stock Keeping Unit) — código único
/// de identificação do produto no sistema de estoque.
/// Imutável e autovalidável: garante que todo SKU criado esteja em formato válido,
/// seguindo o padrão de Value Objects do DDD onde a validade é intrínseca ao objeto.
/// </summary>
public sealed partial record Sku
{
    // O valor normalizado do SKU (maiúsculo, sem espaços)
    public string Value { get; }

    /// <summary>
    /// Cria um SKU validando formato, tamanho e normalizando para maiúsculo.
    /// A validação no construtor garante que SKUs inválidos nem sequer existam
    /// no sistema — principio de "fail fast".
    /// </summary>
    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be empty", nameof(value));

        // Normaliza: remove espaços extras e converte para maiúsculo
        // Isso evita duplicatas como "abc-123" e "ABC-123"
        value = value.Trim().ToUpperInvariant();

        // Garante que o SKU contém apenas caracteres permitidos
        if (!SkuRegex().IsMatch(value))
            throw new ArgumentException("SKU must contain only letters, numbers, hyphens and underscores", nameof(value));

        // Tamanho mínimo e máximo para evitar códigos muito curtos ou absurdamente longos
        if (value.Length < 3 || value.Length > 50)
            throw new ArgumentException("SKU must be between 3 and 50 characters", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;

    // Regex compilado em tempo de compilação (source generator) para performance
    [GeneratedRegex(@"^[A-Z0-9\-_]+$")]
    private static partial Regex SkuRegex();
}
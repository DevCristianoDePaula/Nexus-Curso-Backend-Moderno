namespace Nexus.Catalog.Domain;

/// <summary>
/// Value Object que representa um valor monetário com sua moeda.
/// Imutável por definição (record), garantindo que um preço nunca seja alterado
/// após criado — se o preço muda, um novo objeto Money é criado.
/// Isso elimina bugs comuns como esquecer de converter moedas ou permitir valores negativos.
/// </summary>
public sealed record Money(decimal Amount, string Currency)
{
    // Garante que o valor monetário nunca seja negativo — um preço negativo não faz sentido no domínio
    public decimal Amount { get; } = Amount < 0
        ? throw new ArgumentException("Amount cannot be negative", nameof(Amount))
        : Amount;

    // Normaliza a moeda para maiúsculas (ex: "brl" → "BRL") e valida que foi informada
    public string Currency { get; } = string.IsNullOrWhiteSpace(Currency)
        ? throw new ArgumentException("Currency is required", nameof(Currency))
        : Currency.ToUpperInvariant();

    // Valor zero na moeda padrão (BRL) — útil para inicializações e comparações
    public static Money Zero(string currency = "BRL") => new(0, currency);

    // Permite usar Money onde um decimal é esperado (ex: cálculos de total)
    public static implicit operator decimal(Money money) => money.Amount;
}
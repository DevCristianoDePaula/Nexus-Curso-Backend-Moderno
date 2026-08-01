namespace Nexus.Shared.Kernel.ValueObjects;

/// <summary>
/// Classe base para Value Objects (DDD).
/// Diferente de entidades, Value Objects são definidos pelos seus atributos,
/// não por uma identidade. Dois Value Objects com os mesmos valores são iguais.
/// Ex: Money(10, "BRL") == Money(10, "BRL").
/// Devem ser imutáveis — depois de criados, não mudam.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Retorna os componentes que definem a igualdade deste Value Object.
    /// Subclasses devem implementar retornando todas as propriedades que
    /// devem ser comparadas (ex: Amount e Currency para Money).
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>Comparação por valor: dois VOs são iguais se todos os componentes forem iguais.</summary>
    public bool Equals(ValueObject? other) =>
        other is not null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    /// <summary>Hash code baseado nos componentes (XOR dos hashes individuais).</summary>
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);

    public static bool operator ==(ValueObject? a, ValueObject? b) =>
        a is null && b is null || a is not null && a.Equals(b);

    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}

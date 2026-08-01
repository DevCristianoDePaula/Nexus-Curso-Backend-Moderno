namespace Nexus.Shared.Kernel.Entities;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Uma entidade é definida por sua identidade (Id), não por seus atributos.
/// Duas entidades com o mesmo Id são consideradas iguais, mesmo que outros
/// atributos difiram. Isso segue o padrão DDD (Domain-Driven Design).
/// </summary>
public abstract class Entity
{
    /// <summary>Identificador único da entidade, gerado automaticamente.</summary>
    public Guid Id { get; protected set; }

    /// <summary>Data/hora de criação (UTC), setada uma vez no construtor.</summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>Data/hora da última alteração (UTC), atualizada via Touch().</summary>
    public DateTime UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    /// <summary>Marca a entidade como modificada (atualiza UpdatedAt).</summary>
    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}

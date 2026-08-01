using System.Linq.Expressions;

namespace Nexus.Shared.Kernel.Specifications;

/// <summary>
/// Interface do padrão Specification (DDD).
/// Permite encapsular regras de negócio reutilizáveis em objetos
/// que podem ser combinados com AND/OR.
/// Ex: "cupom ativo" + "cupom dentro da validade" + "compra mínima atingida".
/// </summary>
public interface ISpecification<T>
{
    /// <summary>Expressão de filtro para consultas (EF Core, MongoDB).</summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>Includes para carregamento eager (EF Core).</summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>Ordenação opcional.</summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>Verifica se a entidade satisfaz a especificação em memória.</summary>
    bool IsSatisfiedBy(T entity);
}

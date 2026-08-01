using System.Linq.Expressions;

namespace Nexus.Shared.Kernel.Specifications;

/// <summary>
/// Implementação base do padrão Specification (DDD).
/// Fornece combinação AND/OR e conversão para Expression (usado em consultas EF Core/MongoDB).
/// Ex de uso: new ActiveCouponSpecification().And(new ValidDateCouponSpecification())
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>Critério de filtro (expressão lambda).</summary>
    public Expression<Func<T, bool>> Criteria { get; }

    /// <summary>Includes para navegação (EF Core).</summary>
    public List<Expression<Func<T, object>>> Includes { get; } = [];

    /// <summary>Ordenação opcional.</summary>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    protected Specification(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    /// <summary>Verifica se a entidade satisfaz a regra em memória.</summary>
    public bool IsSatisfiedBy(T entity) => Criteria.Compile()(entity);

    /// <summary>Combina duas specifications com AND lógico.</summary>
    public Specification<T> And(Specification<T> other) =>
        new AndSpecification<T>(this, other);

    /// <summary>Combina duas specifications com OR lógico.</summary>
    public Specification<T> Or(Specification<T> other) =>
        new OrSpecification<T>(this, other);

    /// <summary>Adiciona Include para carregamento eager.</summary>
    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);

    /// <summary>Define ordenação.</summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;
}

/// <summary>Combina duas specifications com AND (Expression.AndAlso).</summary>
public class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
        : base(Combine(left.Criteria, right.Criteria))
    {
        _left = left;
        _right = right;
    }

    /// <summary>Combina duas expressões lambda com AND, ajustando os parâmetros.</summary>
    private static Expression<Func<T, bool>> Combine(
        Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var body = Expression.AndAlso(left.Body, Expression.Invoke(right, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

/// <summary>Combina duas specifications com OR (Expression.OrElse).</summary>
public class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
        : base(Combine(left.Criteria, right.Criteria))
    {
        _left = left;
        _right = right;
    }

    /// <summary>Combina duas expressões lambda com OR, ajustando os parâmetros.</summary>
    private static Expression<Func<T, bool>> Combine(
        Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var body = Expression.OrElse(left.Body, Expression.Invoke(right, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

using System.Linq.Expressions;

namespace GAIA.Core.Queries;

public record PaginatedQuery
{
  public int Limit { get; init; } = -1;
  public int Offset { get; init; } = -1;

  public bool HasPagination =>
      Limit > 0 && Offset >= 0;
}

public record FindManyOpts<T> where T : IdentityObject
{
  // SAFE: EF Core-friendly ordering
  public Expression<Func<T, object>> OrderFunc { get; init; } = e => e.Id;

  // EF-safe default filter
  public Expression<Func<T, bool>> WhereFunc { get; init; } = _ => true;

  // Use IReadOnlyCollection to avoid unintended mutation
  public IReadOnlyCollection<Expression<Func<T, object>>> IncludeFuncs { get; init; }
      = Array.Empty<Expression<Func<T, object>>>();
}

public record FindManyAndSelectOpts<T, TResult> : FindManyOpts<T>
    where T : IdentityObject
{
  public Expression<Func<T, TResult>> SelectFunc { get; init; }
}

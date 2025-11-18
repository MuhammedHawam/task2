using GAIA.Core;
using GAIA.Core.Interfaces;
using GAIA.Core.Queries;
using System.Linq.Expressions;

namespace GAIA.Tests.Mocks;

public class MockRepository<T>(ICollection<T> store) : IRepository<T>
  where T : IdentityObject
{
  public void AddElements(ICollection<T> elements)
  {
    elements.ToList().ForEach(store.Add);
  }

  /// <inheritdoc />
  public Task<T> FindOneById(Guid id)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc />
  public Task<T> FindOne(Expression<Func<T, bool>> whereFunc)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc />
  public Task<int> Count(Expression<Func<T, bool>> whereFunc)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc />
  public Task<List<TResult>> FindAndSelect<TResult>(FindManyAndSelectOpts<T, TResult> opts)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc />
  public Task<List<T>> FindMany(PaginatedQuery query)
  {
    return Task.FromResult(store.ToList());
  }

  /// <inheritdoc />
  public Task<List<T>> FindMany(PaginatedQuery query, FindManyOpts<T> opts)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc />
  public Task Upsert(T instance)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc />
  public Task UpsertMany(IEnumerable<T> instances)
  {
    throw new NotImplementedException();
  }
}

using GAIA.Core.Queries;
using System.Linq.Expressions;

namespace GAIA.Core.Interfaces;

public interface IRepository<T> where T : IdentityObject
{
  Task<T> FindOneById(Guid Id);
  Task<T> FindOne(Expression<Func<T, bool>> whereFunc);
  Task<int> Count(Expression<Func<T, bool>> whereFunc);
  Task<List<TResult>> FindAndSelect<TResult>(FindManyAndSelectOpts<T, TResult> opts);
  Task<List<T>> FindMany(PaginatedQuery query);
  Task<List<T>> FindMany(PaginatedQuery query, FindManyOpts<T> opts);
  Task Upsert(T instance);
  Task UpsertMany(IEnumerable<T> instances);
}

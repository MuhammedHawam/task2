using GAIA.Domain;
using GAIA.Core.Queries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GAIA.Infra.EFCore.Repositories;

public abstract class BaseRepository<T>(DbContext context) where T : IdentityObject
{
  protected readonly DbContext Ctx = context;
  protected readonly DbSet<T> Root = context.Set<T>();

  // -- READ operations: use AsNoTracking() for performance on read-only queries --

  public virtual Task<T?> FindOneById(Guid id)
  {
    // SingleOrDefault is more correct for key lookups: duplicates indicate data corruption.
    return Root
        .AsNoTracking()
        .SingleOrDefaultAsync(agg => agg.Id == id);
  }

  public virtual Task<T?> FindOne(Expression<Func<T, bool>> whereFunc)
  {
    return Root
        .AsNoTracking()
        .Where(whereFunc)
        .FirstOrDefaultAsync(); // OK here because where can be non-unique
  }

  public virtual Task<int> Count(Expression<Func<T, bool>> whereFunc)
  {
    return Root.AsNoTracking().CountAsync(whereFunc);
  }

  public virtual async Task<List<TResult>> FindAndSelect<TResult>(FindManyAndSelectOpts<T, TResult> opts)
  {
    // Materialize as List<TResult> to make behavior explicit
    var query = Root.AsNoTracking().Where(opts.WhereFunc);

    // If there are includes, apply them safely (split query if several includes to avoid join explosion)
    if (opts.IncludeFuncs != null && opts.IncludeFuncs.Any())
    {
      foreach (var inc in opts.IncludeFuncs)
        query = query.Include(inc);

      if (opts.IncludeFuncs.Count > 1)
        query = query.AsSplitQuery();
    }

    // Apply ordering BEFORE pagination (if the Select requires ordering on original type)
    if (opts.OrderFunc is not null)
      query = query.OrderBy(opts.OrderFunc);

    // Select and return
    return await query.Select(opts.SelectFunc).ToListAsync();
  }

  public virtual Task<List<T>> FindMany(PaginatedQuery query)
  {
    return FindMany(query, new FindManyOpts<T>());
  }

  public virtual async Task<List<T>> FindMany(PaginatedQuery query, FindManyOpts<T> opts)
  {
    IQueryable<T> queryTool = Root.AsNoTracking();

    // Apply includes early — that allows EF to optimize; use AsSplitQuery when there are many includes.
    if (opts.IncludeFuncs != null && opts.IncludeFuncs.Any())
    {
      foreach (var ifunc in opts.IncludeFuncs)
      {
        queryTool = queryTool.Include(ifunc);
      }

      if (opts.IncludeFuncs.Count > 1)
      {
        // AsSplitQuery avoids giant single SQL with many joins which can cause Cartesian explosion
        queryTool = queryTool.AsSplitQuery();
      }
    }

    // Apply filter
    queryTool = queryTool.Where(opts.WhereFunc);

    // Apply ordering BEFORE Skip/Take — critical for stable pagination
    if (opts.OrderFunc is not null)
      queryTool = queryTool.OrderBy(opts.OrderFunc);

    // Apply pagination
    if (query.Offset > 0)
      queryTool = queryTool.Skip(query.Offset);

    if (query.Limit > 0)
      queryTool = queryTool.Take(query.Limit);

    // Materialize as List<T>
    return await queryTool.ToListAsync();
  }

  // -- UPSERT operations: explicit existence checks to avoid accidental overwrites --

  public virtual async Task Upsert(T entity)
  {
    if (entity == null) throw new ArgumentNullException(nameof(entity));

    // If the entity has default id, treat as new
    if (entity.Id == Guid.Empty)
    {
      // create new id if your domain expects it
      entity.Id = Guid.NewGuid();
      Root.Add(entity);
      await Ctx.SaveChangesAsync();
      return;
    }

    // Check if the entity exists in the database
    var exists = await Root.AsNoTracking().AnyAsync(e => e.Id == entity.Id);
    if (!exists)
    {
      Root.Add(entity);
    }
    else
    {
      // Attach & mark modified to ensure proper Update semantics with change tracking
      var attached = Ctx.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == entity.Id);
      if (attached != null)
      {
        // copy values into tracked entity to preserve concurrency tokens if present
        Ctx.Entry(attached.Entity).CurrentValues.SetValues(entity);
      }
      else
      {
        // Attach the entity and mark as Modified
        Root.Attach(entity);
        Ctx.Entry(entity).State = EntityState.Modified;
      }
    }

    await Ctx.SaveChangesAsync();
  }

  public virtual async Task UpsertMany(IEnumerable<T> entities)
  {
    if (entities == null) throw new ArgumentNullException(nameof(entities));

    var entityList = entities.ToList();
    if (!entityList.Any()) return;

    // Collect IDs for existing check — single DB roundtrip
    var ids = entityList.Where(e => e.Id != Guid.Empty).Select(e => e.Id).ToList();
    var existingIds = new HashSet<Guid>();
    if (ids.Any())
    {
      existingIds = (await Root.AsNoTracking().Where(e => ids.Contains(e.Id)).Select(e => e.Id).ToListAsync())
                    .ToHashSet();
    }

    var toAdd = new List<T>();
    foreach (var entity in entityList)
    {
      if (entity.Id == Guid.Empty)
      {
        entity.Id = Guid.NewGuid();
        toAdd.Add(entity);
        continue;
      }

      if (!existingIds.Contains(entity.Id))
      {
        toAdd.Add(entity);
      }
      else
      {
        // If tracked copy exists, merge; otherwise attach and mark modified
        var attached = Ctx.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == entity.Id);
        if (attached != null)
        {
          Ctx.Entry(attached.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
          Root.Attach(entity);
          Ctx.Entry(entity).State = EntityState.Modified;
        }
      }
    }

    if (toAdd.Any())
      Root.AddRange(toAdd);

    await Ctx.SaveChangesAsync();
  }
}

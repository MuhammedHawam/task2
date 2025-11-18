using GAIA.Core.Interfaces;
using GAIA.Core.Queries;
using GAIA.Domain.Framework;

namespace GAIA.Infra.EFCore.Repositories;

public class FrameworkRepository(GaiaDbContext context)
  : BaseRepository<Framework>(context), IRepository<Framework>
{
  public override async Task<List<Framework>> FindMany(PaginatedQuery query)
  {
    return await FindMany(
      query,
      new FindManyOpts<Framework> { IncludeFuncs = [f => f.Depths, f => f.Scorings] }
    );
  }
}

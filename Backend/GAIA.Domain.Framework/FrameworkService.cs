using GAIA.Core.DTOs;
using GAIA.Core.Interfaces;
using GAIA.Core.Queries;

namespace GAIA.Domain.Framework;

public class FrameworkService(IRepository<Framework> frameworkRepository) : IFrameworkService
{
  public async Task<IEnumerable<FrameworkConfigurationOption>> ListFrameworksWithOptions()
  {
    var frameworks = await frameworkRepository.FindMany(new PaginatedQuery());

    return frameworks.Select(framework =>
    {
      var depths = framework.Depths
        .OrderBy(depth => depth.Depth)
        .ThenBy(depth => depth.Name)
        .Select(depth => new FrameworkDepthOption(depth.Id, depth.Name, depth.Depth));

      var scorings = framework.Scorings
        .OrderBy(depth => depth.Name)
        .Select(scoring => new ScoringOption(scoring.Id, scoring.Name));

      return new FrameworkConfigurationOption(framework.Id, framework.Title, depths, scorings);
    });
  }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Core.Configuration.Interfaces;
using GAIA.Core.Configuration.Models;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework.Entities;

namespace GAIA.Core.Services.Configuration
{
  public class AssessmentConfigurationService : IAssessmentConfigurationService
  {
    private readonly IAssessmentConfigurationDataSource _dataSource;

    public AssessmentConfigurationService(IAssessmentConfigurationDataSource dataSource)
    {
      _dataSource = dataSource;
    }

    public async Task<AssessmentConfigurationOptions> GetOptionsAsync(CancellationToken cancellationToken)
    {
      var frameworksTask = _dataSource.GetFrameworksAsync(cancellationToken);
      var depthsTask = _dataSource.GetAssessmentDepthsAsync(cancellationToken);
      var scoringsTask = _dataSource.GetAssessmentScoringsAsync(cancellationToken);

      await Task.WhenAll(frameworksTask, depthsTask, scoringsTask);

      var frameworks = frameworksTask.Result;
      var depths = depthsTask.Result;
      var scorings = scoringsTask.Result;

      var depthsLookup = depths
        .GroupBy(d => d.FrameworkId)
        .ToDictionary(g => g.Key, g => g.ToList());

      var scoringLookup = scorings
        .GroupBy(s => s.AssessmentDepthId)
        .ToDictionary(g => g.Key, g => g.ToList());

      var frameworkOptions = frameworks
        .Select(framework =>
        {
          var depthOptions = depthsLookup.TryGetValue(framework.Id, out var depthList)
            ? depthList
                .OrderBy(depth => depth.Name)
                .Select(depth =>
                  new AssessmentDepthOption(
                    depth.Id,
                    depth.Name,
                    scoringLookup.TryGetValue(depth.Id, out var scoringList)
                      ? scoringList
                          .OrderBy(scoring => scoring.Name)
                          .Select(scoring => new AssessmentScoringOption(scoring.Id, scoring.Name))
                          .ToList()
                      : new List<AssessmentScoringOption>()))
                .ToList()
            : new List<AssessmentDepthOption>();

          return new FrameworkConfigurationOption(
            framework.Id,
            framework.Title ?? string.Empty,
            depthOptions
          );
        })
        .OrderBy(option => option.Name)
        .ToList();

      return new AssessmentConfigurationOptions(frameworkOptions);
    }
  }
}

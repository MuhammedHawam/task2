using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.Assessment.Models;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Core.Services.Configuration
{
  public class AssessmentConfigurationService : IAssessmentConfigurationService
  {
    private readonly IDocumentSession? _session;
    private readonly IReadOnlyList<GAIA.Domain.Framework.Entities.Framework>? _frameworkOverrides;
    private readonly IReadOnlyList<AssessmentDepth>? _depthOverrides;
    private readonly IReadOnlyList<AssessmentScoring>? _scoringOverrides;

    public AssessmentConfigurationService(IDocumentSession session)
    {
      _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    // Constructor used for unit testing to inject in-memory data instead of a Marten session
    public AssessmentConfigurationService(
      IReadOnlyList<GAIA.Domain.Framework.Entities.Framework> frameworks,
      IReadOnlyList<AssessmentDepth> assessmentDepths,
      IReadOnlyList<AssessmentScoring> assessmentScorings)
    {
      _session = null;
      _frameworkOverrides = frameworks ?? throw new ArgumentNullException(nameof(frameworks));
      _depthOverrides = assessmentDepths ?? throw new ArgumentNullException(nameof(assessmentDepths));
      _scoringOverrides = assessmentScorings ?? throw new ArgumentNullException(nameof(assessmentScorings));
    }

    public async Task<AssessmentConfigurationOptions> GetOptionsAsync(CancellationToken cancellationToken)
    {
      IReadOnlyList<GAIA.Domain.Framework.Entities.Framework> frameworks;
      IReadOnlyList<AssessmentDepth> depths;
      IReadOnlyList<AssessmentScoring> scorings;

      if (_frameworkOverrides is not null && _depthOverrides is not null && _scoringOverrides is not null)
      {
        frameworks = _frameworkOverrides;
        depths = _depthOverrides;
        scorings = _scoringOverrides;
      }
      else
      {
        if (_session is null)
        {
          throw new InvalidOperationException("AssessmentConfigurationService requires a valid session when overrides are not provided.");
        }

        var frameworksTask = _session.Query<GAIA.Domain.Framework.Entities.Framework>().ToListAsync(cancellationToken);
        var depthsTask = _session.Query<AssessmentDepth>().ToListAsync(cancellationToken);
        var scoringsTask = _session.Query<AssessmentScoring>().ToListAsync(cancellationToken);

        await Task.WhenAll(frameworksTask, depthsTask, scoringsTask);

        frameworks = await frameworksTask;
        depths = await depthsTask;
        scorings = await scoringsTask;
      }

      var depthsLookup = depths
        .GroupBy(d => d.FrameworkId)
        .ToDictionary(g => g.Key, g => g.ToList());

      var scoringLookup = scorings
        .GroupBy(s => s.FrameworkId)
        .ToDictionary(
          g => g.Key,
          g => (IReadOnlyList<AssessmentScoringOption>)g
            .OrderBy(scoring => scoring.Name)
            .Select(scoring => new AssessmentScoringOption(scoring.Id, scoring.Name))
            .ToList());

      var frameworkOptions = frameworks
        .Select(framework =>
        {
          var scoringOptions = scoringLookup.TryGetValue(framework.Id, out var frameworkScorings)
            ? frameworkScorings
            : Array.Empty<AssessmentScoringOption>();

          var depthOptions = depthsLookup.TryGetValue(framework.Id, out var depthList)
            ? depthList
                .OrderBy(depth => depth.Depth)
                .ThenBy(depth => depth.Name)
                .Select(depth =>
                  new AssessmentDepthOption(
                    depth.Id,
                    depth.Name,
                    depth.Depth))
                .ToList()
            : new List<AssessmentDepthOption>();

          return new FrameworkConfigurationOption(
            framework.Id,
            framework.Title,
            depthOptions,
            scoringOptions
          );
        })
        .OrderBy(option => option.Name)
        .ToList();

      return new AssessmentConfigurationOptions(frameworkOptions);
    }
  }
}

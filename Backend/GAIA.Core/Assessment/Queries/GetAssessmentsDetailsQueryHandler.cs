using GAIA.Domain.Assessment.Entities;
using Marten;
using MediatR;

namespace GAIA.Core.Assessment.Queries
{
  public class GetAssessmentsDetailsQueryHandler : IRequestHandler<GetAssessmentsDetailsQuery, IReadOnlyList<AssessmentDetails>>
  {
    private readonly IQuerySession _querySession;

    public GetAssessmentsDetailsQueryHandler(IQuerySession querySession)
    {
      _querySession = querySession;
    }

    public async Task<IReadOnlyList<AssessmentDetails>> Handle(GetAssessmentsDetailsQuery request, CancellationToken cancellationToken)
    {
      var assessments = await _querySession.Query<Domain.Assessment.Entities.AssessmentDetails>().ToListAsync(cancellationToken);

      if (assessments.Count == 0)
      {
        return Array.Empty<AssessmentDetails>();
      }

      var depthIds = assessments
        .Select(assessment => assessment.AssessmentDepthId)
        .Where(id => id != Guid.Empty)
        .Distinct()
        .ToArray();

      var scoringIds = assessments
        .Select(assessment => assessment.AssessmentScoringId)
        .Where(id => id != Guid.Empty)
        .Distinct()
        .ToArray();

      var depthsTask = depthIds.Length > 0
        ? _querySession.Query<AssessmentDepth>()
          .Where(depth => depthIds.Contains(depth.Id))
          .ToListAsync(cancellationToken)
        : Task.FromResult<IReadOnlyList<AssessmentDepth>>(Array.Empty<AssessmentDepth>());

      var scoringsTask = scoringIds.Length > 0
        ? _querySession.Query<AssessmentScoring>()
          .Where(scoring => scoringIds.Contains(scoring.Id))
          .ToListAsync(cancellationToken)
        : Task.FromResult<IReadOnlyList<AssessmentScoring>>(Array.Empty<AssessmentScoring>());

      await Task.WhenAll(depthsTask, scoringsTask);

      var depthLookup = depthsTask.Result.ToDictionary(depth => depth.Id);
      var scoringLookup = scoringsTask.Result.ToDictionary(scoring => scoring.Id);

      var results = assessments
        .Select(assessment => new AssessmentDetails(
          assessment,
          depthLookup.TryGetValue(assessment.AssessmentDepthId, out var depth) ? depth : null,
          scoringLookup.TryGetValue(assessment.AssessmentScoringId, out var scoring) ? scoring : null
        ))
        .ToList();

      return results;
    }
  }

}

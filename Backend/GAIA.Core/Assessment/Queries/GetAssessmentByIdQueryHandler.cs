using GAIA.Domain.Assessment.Entities;
using Marten;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentByIdQueryHandler : IRequestHandler<GetAssessmentByIdQuery, AssessmentDetails?>
{
  private readonly IQuerySession _querySession;

  public GetAssessmentByIdQueryHandler(IQuerySession querySession)
  {
    _querySession = querySession;
  }

  public async Task<AssessmentDetails?> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
  {
    var assessmentTask = _querySession.LoadAsync<Domain.Assessment.Entities.Assessment>(request.AssessmentId, cancellationToken);
    var assessment = await assessmentTask;

    if (assessment is null)
    {
      return null;
    }

    var depthTask = assessment.AssessmentDepthId != Guid.Empty
      ? _querySession.LoadAsync<AssessmentDepth>(assessment.AssessmentDepthId, cancellationToken)
      : Task.FromResult<AssessmentDepth?>(null);

    var scoringTask = assessment.AssessmentScoringId != Guid.Empty
      ? _querySession.LoadAsync<AssessmentScoring>(assessment.AssessmentScoringId, cancellationToken)
      : Task.FromResult<AssessmentScoring?>(null);

    await Task.WhenAll(depthTask, scoringTask);

    return new AssessmentDetails(
      assessment,
      depthTask.Result,
      scoringTask.Result);
  }
}

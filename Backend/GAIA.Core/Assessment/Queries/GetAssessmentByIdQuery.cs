using System;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Domain.Assessment.Entities;
using Marten;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public sealed class GetAssessmentByIdQuery : IRequest<AssessmentDetails?>
{
  public GetAssessmentByIdQuery(Guid assessmentId)
  {
    AssessmentId = assessmentId;
  }

  public Guid AssessmentId { get; }
}

public class GetAssessmentByIdQueryHandler : IRequestHandler<GetAssessmentByIdQuery, AssessmentDetails?>
{
  private readonly IQuerySession _querySession;

  public GetAssessmentByIdQueryHandler(IQuerySession querySession)
  {
    _querySession = querySession;
  }

  public async Task<AssessmentDetails?> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
  {
    var assessment = await _querySession.LoadAsync<Assessment>(request.AssessmentId, cancellationToken);

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
      scoringTask.Result
    );
  }
}

using GAIA.Domain.Assessment.Entities;
using Marten;
using Marten.Linq;
using MediatR;
using System.Linq;

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
    AssessmentDepth? depth;
    AssessmentScoring? scoring;

    var assessment = await _querySession.Query<Domain.Assessment.Entities.Assessment>()
      .Where(a => a.Id == request.AssessmentId)
      .Include(a => a.AssessmentDepthId, out depth)
      .Include(a => a.AssessmentScoringId, out scoring)
      .SingleOrDefaultAsync(cancellationToken);

    if (assessment is null)
    {
      return null;
    }

    return new AssessmentDetails(
      assessment,
      depth,
      scoring);
  }
}

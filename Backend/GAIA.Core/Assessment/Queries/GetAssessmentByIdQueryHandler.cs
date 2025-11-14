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
    AssessmentDepth? depth = null;
    AssessmentScoring? scoring = null;

    var query = _querySession.Query<Domain.Assessment.Entities.Assessment>()
      .Include<AssessmentDepth>(a => a.AssessmentDepthId, loaded => { depth = loaded; })
      .Include<AssessmentScoring>(a => a.AssessmentScoringId, loaded => { scoring = loaded; });

    var assessment = await query
      .Where(a => a.Id == request.AssessmentId)
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

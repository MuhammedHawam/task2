using GAIA.Domain.Assessment.Entities;
using Marten;
using MediatR;

namespace GAIA.Core.Assessment.Queries
{
  public class GetAssessmentDetailsByIdQueryHandler : IRequestHandler<GetAssessmentDetailsByIdQuery, AssessmentDetails?>
  {
    private readonly IQuerySession _querySession;

    public GetAssessmentDetailsByIdQueryHandler(IQuerySession querySession)
    {
      _querySession = querySession;
    }

    public async Task<AssessmentDetails?> Handle(GetAssessmentDetailsByIdQuery request, CancellationToken cancellationToken)
    {
      AssessmentDepth? depth = null;
      AssessmentScoring? scoring = null;

      var query = _querySession.Query<Domain.Assessment.Entities.AssessmentDetails>()
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
        scoring
      );
    }
  }
}

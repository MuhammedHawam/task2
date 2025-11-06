using GAIA.Domain.Assessment.Entities;
using Marten;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentByIdQuery(Guid AssessmentId) : IRequest<Assessment?>;

public class GetAssessmentByIdQueryHandler : IRequestHandler<GetAssessmentByIdQuery, Assessment?>
{
  private readonly IQuerySession _querySession;

  public GetAssessmentByIdQueryHandler(IQuerySession querySession)
  {
    _querySession = querySession;
  }

  public async Task<Assessment?> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
  {
    return await _querySession.LoadAsync<Assessment>(request.AssessmentId, cancellationToken);
  }
}

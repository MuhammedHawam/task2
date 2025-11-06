using GAIA.Domain.Assessment.Entities;
using Marten;
using Marten.Linq;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentsQuery() : IRequest<IReadOnlyList<Assessment>>;

public class GetAssessmentsQueryHandler : IRequestHandler<GetAssessmentsQuery, IReadOnlyList<Assessment>>
{
  private readonly IQuerySession _querySession;

  public GetAssessmentsQueryHandler(IQuerySession querySession)
  {
    _querySession = querySession;
  }

  public async Task<IReadOnlyList<Assessment>> Handle(GetAssessmentsQuery request, CancellationToken cancellationToken)
  {
    var assessments = await _querySession.Query<Assessment>().ToListAsync(cancellationToken);
    return assessments;
  }
}

using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentAccessUsersQueryHandler
  : IRequestHandler<GetAssessmentAccessUsersQuery, IReadOnlyList<AssessmentUserAccess>>
{
  public Task<IReadOnlyList<AssessmentUserAccess>> Handle(
    GetAssessmentAccessUsersQuery request,
    CancellationToken cancellationToken)
  {
    // TODO: Integrate with the definitive user directory/permissions service once available.
    IReadOnlyList<AssessmentUserAccess> users = Array.Empty<AssessmentUserAccess>();
    return Task.FromResult(users);
  }
}

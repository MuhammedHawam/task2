using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentAccessUsersQueryHandler
  : IRequestHandler<GetAssessmentAccessUsersQuery, IReadOnlyList<AssessmentUserAccess>>
{
  private static readonly IReadOnlyList<AssessmentUserAccess> SeedUsers =
    new[]
    {
      new AssessmentUserAccess(
        Guid.Parse("82d90f89-e8c4-4ee5-88aa-2d8d0c7b84de"),
        "jane.doe",
        "jane.doe@example.com",
        "https://static.gaia.local/avatars/jane.png",
        "Admin"
      ),
      new AssessmentUserAccess(
        Guid.Parse("5ff6262f-ff8d-4c71-9f5f-8fd5c0f9fca3"),
        "marco.rossi",
        "marco.rossi@example.com",
        "https://static.gaia.local/avatars/marco.png",
        "Editor"
      ),
      new AssessmentUserAccess(
        Guid.Parse("4eb92a45-073b-4c38-8808-636dd2b444fc"),
        "lydia.chen",
        "lydia.chen@example.com",
        "https://static.gaia.local/avatars/lydia.png",
        "Viewer"
      )
    };

  public Task<IReadOnlyList<AssessmentUserAccess>> Handle(
    GetAssessmentAccessUsersQuery request,
    CancellationToken cancellationToken)
  {
    // TODO: Replace with integration to the user management source once it exists.
    return Task.FromResult(SeedUsers);
  }
}

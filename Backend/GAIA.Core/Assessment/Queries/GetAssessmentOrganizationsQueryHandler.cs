using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentOrganizationsQueryHandler
  : IRequestHandler<GetAssessmentOrganizationsQuery, IReadOnlyList<AssessmentOrganization>>
{
  private static readonly IReadOnlyList<AssessmentOrganization> SeedOrganizations =
    new[]
    {
      new AssessmentOrganization(
        Guid.Parse("51c1cf73-3643-4b2a-a6ea-7b7e00a4df9b"),
        "Contoso Manufacturing",
        "https://static.gaia.local/images/orgs/contoso.png",
        "https://contoso-manufacturing.com",
        "Global producer of industrial components."
      ),
      new AssessmentOrganization(
        Guid.Parse("2a7b4b2d-7b0d-4d2f-9cfa-33dbb2be95d2"),
        "Fabrikam Health",
        "https://static.gaia.local/images/orgs/fabrikam.png",
        "https://fabrikam-health.com",
        "Regional healthcare network with hospitals and labs."
      ),
      new AssessmentOrganization(
        Guid.Parse("9eb3d187-5f5e-4a4e-8ea6-0c1a8a1e53c5"),
        "Northwind Traders",
        "https://static.gaia.local/images/orgs/northwind.png",
        "https://northwind-traders.com",
        "Supply-chain and logistics partner for retail brands."
      )
    };

  public Task<IReadOnlyList<AssessmentOrganization>> Handle(
    GetAssessmentOrganizationsQuery request,
    CancellationToken cancellationToken)
  {
    // TODO: Replace with persistent organization management once implemented.
    return Task.FromResult(SeedOrganizations);
  }
}

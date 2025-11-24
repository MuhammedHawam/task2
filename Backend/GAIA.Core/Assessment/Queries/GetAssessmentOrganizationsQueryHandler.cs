using System.Security.Cryptography;
using System.Text;
using GAIA.Core.Assessment.Interfaces;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentOrganizationsQueryHandler
  : IRequestHandler<GetAssessmentOrganizationsQuery, IReadOnlyList<AssessmentOrganization>>
{
  private readonly IAssessmentRepository _assessmentRepository;

  public GetAssessmentOrganizationsQueryHandler(IAssessmentRepository assessmentRepository)
  {
    _assessmentRepository = assessmentRepository;
  }

  public async Task<IReadOnlyList<AssessmentOrganization>> Handle(
    GetAssessmentOrganizationsQuery request,
    CancellationToken cancellationToken)
  {
    var assessments = await _assessmentRepository.ListAsync(cancellationToken);

    var organizations = assessments
      .Select(assessment => assessment.Organization?.Trim())
      .Where(name => !string.IsNullOrWhiteSpace(name))
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .Select(name => new AssessmentOrganization(
        CreateDeterministicGuid(name!),
        name!,
        null,
        null,
        null
      ))
      .OrderBy(org => org.Name, StringComparer.OrdinalIgnoreCase)
      .ToList();

    return organizations;
  }

  private static Guid CreateDeterministicGuid(string input)
  {
    var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
    return new Guid(hash);
  }
}

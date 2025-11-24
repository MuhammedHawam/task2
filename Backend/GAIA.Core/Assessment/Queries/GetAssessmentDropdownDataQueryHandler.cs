using System.Security.Cryptography;
using System.Text;
using GAIA.Core.Assessment.Interfaces;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentDropdownDataQueryHandler
  : IRequestHandler<GetAssessmentDropdownDataQuery, AssessmentDropdownData>
{
  private readonly IAssessmentRepository _assessmentRepository;

  public GetAssessmentDropdownDataQueryHandler(IAssessmentRepository assessmentRepository)
  {
    _assessmentRepository = assessmentRepository;
  }

  public async Task<AssessmentDropdownData> Handle(
    GetAssessmentDropdownDataQuery request,
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

    var languages = assessments
      .Select(assessment => assessment.Language?.Trim())
      .Where(language => !string.IsNullOrWhiteSpace(language))
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .OrderBy(language => language, StringComparer.OrdinalIgnoreCase)
      .ToList();

    if (languages.Count == 0)
    {
      languages.Add("English");
    }

    return new AssessmentDropdownData(
      organizations,
      languages,
      AssessmentDropdownDefaults.Statuses,
      AssessmentDropdownDefaults.IconTypes,
      AssessmentDropdownDefaults.RoleTypes
    );
  }

  private static Guid CreateDeterministicGuid(string input)
  {
    var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
    return new Guid(hash);
  }
}

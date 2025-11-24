using GAIA.Core.Assessment.Interfaces;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentLanguagesQueryHandler
  : IRequestHandler<GetAssessmentLanguagesQuery, IReadOnlyList<string>>
{
  private readonly IAssessmentRepository _assessmentRepository;

  public GetAssessmentLanguagesQueryHandler(IAssessmentRepository assessmentRepository)
  {
    _assessmentRepository = assessmentRepository;
  }

  public async Task<IReadOnlyList<string>> Handle(
    GetAssessmentLanguagesQuery request,
    CancellationToken cancellationToken)
  {
    var assessments = await _assessmentRepository.ListAsync(cancellationToken);

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

    return languages;
  }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.DTOs.Assessment;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Queries.Assessment
{
  public record GetAssessmentsQuery : IRequest<IReadOnlyList<AssessmentDto>>;

  public class GetAssessmentsQueryHandler : IRequestHandler<GetAssessmentsQuery, IReadOnlyList<AssessmentDto>>
  {
    private readonly IAssessmentRepository _repository;

    public GetAssessmentsQueryHandler(IAssessmentRepository repository)
    {
      _repository = repository;
    }

    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAssessmentsQuery request, CancellationToken cancellationToken)
    {
      var assessments = await _repository.ListAsync(cancellationToken);

      return assessments
        .Select(MapToDto)
        .ToList();
    }

    private static AssessmentDto MapToDto(Assessment assessment)
    {
      return new AssessmentDto(
        assessment.Id,
        assessment.Title,
        assessment.Description,
        assessment.CreatedAt,
        assessment.CreatedBy,
        assessment.FrameworkId,
        assessment.AssessmentDepthId,
        assessment.AssessmentScoringId
      );
    }
  }
}

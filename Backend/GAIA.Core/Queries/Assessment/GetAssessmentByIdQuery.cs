using System;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.DTOs.Assessment;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Queries.Assessment
{
  public record GetAssessmentByIdQuery(Guid AssessmentId) : IRequest<AssessmentDto?>;

  public class GetAssessmentByIdQueryHandler : IRequestHandler<GetAssessmentByIdQuery, AssessmentDto?>
  {
    private readonly IAssessmentRepository _repository;

    public GetAssessmentByIdQueryHandler(IAssessmentRepository repository)
    {
      _repository = repository;
    }

    public async Task<AssessmentDto?> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
    {
      var assessment = await _repository.GetByIdAsync(request.AssessmentId);

      if (assessment is null)
      {
        return null;
      }

      return MapToDto(assessment);
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

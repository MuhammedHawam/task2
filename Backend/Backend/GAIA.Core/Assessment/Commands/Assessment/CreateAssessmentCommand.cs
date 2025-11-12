using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment
{
  public record CreateAssessmentCommand(
     string Title,
     string Description,
     Guid CreatedBy,
     Guid FrameworkId,
     Guid AssessmentDepthId,
     Guid AssessmentScoringId
 ) : IRequest<CreateAssessmentResult>;

  public record CreateAssessmentResult(Guid AssessmentId);
}

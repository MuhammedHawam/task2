using MediatR;

namespace GAIA.Core.Commands.Assessment
{
  public record CreateAssessmentCommand(
     string Title,
     string Description,
     Guid CreatedBy,
     Guid FrameworkId
 ) : IRequest<CreateAssessmentResult>;

  public record CreateAssessmentResult(Guid AssessmentId);
}

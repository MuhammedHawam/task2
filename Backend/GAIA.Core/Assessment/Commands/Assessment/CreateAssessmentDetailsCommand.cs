using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment
{
public record CreateAssessmentDetailsCommand(
   Guid AssessmentId,
   string Title,
   string Description,
   Guid CreatedBy,
   Guid FrameworkId,
   Guid AssessmentDepthId,
   Guid AssessmentScoringId
) : IRequest<CreateAssessmentDetailsResult>;

public record CreateAssessmentDetailsResult(Guid AssessmentId, Guid AssessmentDetailsId);
}

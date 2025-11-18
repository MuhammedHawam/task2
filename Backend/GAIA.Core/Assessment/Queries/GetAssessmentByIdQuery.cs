using MediatR;

namespace GAIA.Core.Assessment.Queries
{
  public record GetAssessmentByIdQuery(Guid AssessmentId) : IRequest<AssessmentDetails?>;
}

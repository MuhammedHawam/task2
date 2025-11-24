using MediatR;

namespace GAIA.Core.Assessment.Queries
{
  public record GetAssessmentDetailsByIdQuery(Guid AssessmentId) : IRequest<AssessmentDetails?>;
}

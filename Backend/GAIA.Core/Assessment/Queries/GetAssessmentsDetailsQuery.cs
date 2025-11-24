using MediatR;

namespace GAIA.Core.Assessment.Queries
{
  public record GetAssessmentsDetailsQuery() : IRequest<IReadOnlyList<AssessmentDetails>>;
}

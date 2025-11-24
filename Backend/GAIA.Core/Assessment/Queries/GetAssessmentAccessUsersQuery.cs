using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentAccessUsersQuery : IRequest<IReadOnlyList<AssessmentUserAccess>>;

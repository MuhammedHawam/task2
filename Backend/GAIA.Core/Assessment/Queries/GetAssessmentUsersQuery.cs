using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentUsersQuery(Guid AssessmentId) : IRequest<AssessmentUserAssignment?>;

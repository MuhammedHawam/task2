using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentOrganizationsQuery : IRequest<IReadOnlyList<AssessmentOrganization>>;

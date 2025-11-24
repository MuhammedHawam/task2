using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentDropdownDataQuery : IRequest<AssessmentDropdownData>;

using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentsQuery : IRequest<IReadOnlyList<Domain.Assessment.Entities.Assessment>>;


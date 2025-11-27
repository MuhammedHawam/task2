using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentLanguagesQuery : IRequest<IReadOnlyList<string>>;

using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public record CreateAssessmentCommand(
  string Name,
  DateTime StartDate,
  DateTime EndDate,
  string Organization,
  string Language
) : IRequest<Domain.Assessment.Entities.Assessment>;

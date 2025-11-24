using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public record UpdateAssessmentCommand(
  Guid AssessmentId,
  string Name,
  DateTime StartDate,
  DateTime EndDate,
  string Organization,
  string Language
) : IRequest<Domain.Assessment.Entities.Assessment?>;

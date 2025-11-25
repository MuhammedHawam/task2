using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public record CreateAssessmentCommand(
  string Name,
  DateTime StartDate,
  DateTime EndDate,
  Guid OrganizationId,
  string Organization,
  string Language
) : IRequest<Domain.Assessment.Entities.Assessment>;

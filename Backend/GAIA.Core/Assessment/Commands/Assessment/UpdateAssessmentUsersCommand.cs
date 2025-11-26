using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public record UpdateAssessmentUsersCommand(
  Guid AssessmentId,
  IReadOnlyCollection<AssessmentUserInput> Users
) : IRequest<AssessmentUserAssignment?>;

using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public record AssignUsersToAssessmentCommand(
  Guid AssessmentId,
  IReadOnlyCollection<AssessmentUserInput> Users
) : IRequest<AssessmentUserAssignment?>;

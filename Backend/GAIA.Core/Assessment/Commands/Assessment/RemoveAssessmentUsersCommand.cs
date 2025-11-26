using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public record RemoveAssessmentUsersCommand(
  Guid AssessmentId,
  IReadOnlyCollection<Guid> UserIds
) : IRequest<AssessmentUserAssignment?>;

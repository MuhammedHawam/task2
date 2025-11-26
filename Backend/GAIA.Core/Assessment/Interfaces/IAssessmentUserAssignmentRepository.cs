using GAIA.Domain.Assessment.Entities;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentUserAssignmentRepository
{
  Task<AssessmentUserAssignment?> GetAsync(Guid assessmentId, CancellationToken cancellationToken);
}

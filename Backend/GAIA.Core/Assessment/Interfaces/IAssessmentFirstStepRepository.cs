using GAIA.Domain.Assessment.Entities;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentFirstStepRepository
{
  Task<AssessmentFirstStep?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

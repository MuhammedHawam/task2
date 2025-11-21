using GAIA.Domain.Assessment.Entities;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentFirstStepRepository
{
  Task<AssessmentFirstStep?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task<IReadOnlyList<AssessmentFirstStep>> ListAsync(CancellationToken cancellationToken);
}

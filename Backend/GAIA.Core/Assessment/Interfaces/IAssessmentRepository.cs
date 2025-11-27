namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentRepository
{
  Task AddAsync(Domain.Assessment.Entities.Assessment assessment, CancellationToken cancellationToken = default);
  Task<Domain.Assessment.Entities.Assessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task UpdateAsync(Domain.Assessment.Entities.Assessment assessment, CancellationToken cancellationToken = default);
  Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

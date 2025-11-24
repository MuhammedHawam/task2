namespace GAIA.Core.Assessment.Interfaces;
public interface IAssessmentRepository
{
  Task<Domain.Assessment.Entities.Assessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task<IReadOnlyList<Domain.Assessment.Entities.Assessment>> ListAsync(CancellationToken cancellationToken);
}

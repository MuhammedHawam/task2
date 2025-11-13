using GAIA.Domain.Documents.Entities;

namespace GAIA.Core.Documents.Interfaces;

public interface IDocumentRepository
{
  Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken);
  Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task AddAsync(Document document, CancellationToken cancellationToken);
  Task UpdateAsync(Document document, CancellationToken cancellationToken);
  Task DeleteAsync(Document document, CancellationToken cancellationToken);
}

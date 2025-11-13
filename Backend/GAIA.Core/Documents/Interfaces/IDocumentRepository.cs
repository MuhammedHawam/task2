using GAIA.Domain.Entities;
using GAIA.Core.Documents.Models;

namespace GAIA.Core.Documents.Interfaces
{
  public interface IDocumentRepository
  {
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentSummary>> GetSummariesAsync(CancellationToken cancellationToken);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool track = false);
    Task<Document?> UpdateAsync(Document document, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
  }
}

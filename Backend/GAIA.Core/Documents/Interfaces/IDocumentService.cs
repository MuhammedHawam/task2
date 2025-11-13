using GAIA.Core.Documents.Models;
using GAIA.Domain.Entities;

namespace GAIA.Core.Documents.Interfaces
{
  public interface IDocumentService
  {
    Task<IReadOnlyList<DocumentSummary>> GetSummariesAsync(CancellationToken cancellationToken);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Document> CreateAsync(string name, string category, string status, byte[] content, CancellationToken cancellationToken);
    Task<Document?> UpdateAsync(Guid id, string name, string category, string status, byte[]? content, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
  }
}

using GAIA.Core.Documents.Models;

namespace GAIA.Core.Documents.Interfaces;

public interface IDocumentService
{
  Task<DocumentCollectionDto> GetAllAsync(CancellationToken cancellationToken);
  Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task<Guid> CreateAsync(CreateDocumentDto document, CancellationToken cancellationToken);
  Task<bool> UpdateAsync(Guid id, UpdateDocumentDto document, CancellationToken cancellationToken);
  Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

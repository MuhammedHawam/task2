using GAIA.Core.Documents.Interfaces;
using GAIA.Core.Documents.Models;
using GAIA.Domain.Entities;

namespace GAIA.Core.Documents.Services
{
  public class DocumentService : IDocumentService
  {
    private readonly IDocumentRepository _repository;

    public DocumentService(IDocumentRepository repository)
    {
      _repository = repository;
    }

    public Task<IReadOnlyList<DocumentSummary>> GetSummariesAsync(CancellationToken cancellationToken)
    {
      return _repository.GetSummariesAsync(cancellationToken);
    }

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
      return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Document> CreateAsync(string name, string category, string status, byte[] content, CancellationToken cancellationToken)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(name);
      ArgumentException.ThrowIfNullOrWhiteSpace(category);
      ArgumentException.ThrowIfNullOrWhiteSpace(status);
      ArgumentNullException.ThrowIfNull(content);

      var document = new Document
      {
        Id = Guid.NewGuid(),
        Name = name,
        Category = category,
        Status = status,
        Content = content
      };

      return await _repository.AddAsync(document, cancellationToken);
    }

    public async Task<Document?> UpdateAsync(Guid id, string name, string category, string status, byte[]? content, CancellationToken cancellationToken)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(name);
      ArgumentException.ThrowIfNullOrWhiteSpace(category);
      ArgumentException.ThrowIfNullOrWhiteSpace(status);

      var document = await _repository.GetByIdAsync(id, cancellationToken, track: true);
      if (document is null)
      {
        return null;
      }

      document.Name = name;
      document.Category = category;
      document.Status = status;

      if (content is not null)
      {
        document.Content = content;
      }

      return await _repository.UpdateAsync(document, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
      return _repository.DeleteAsync(id, cancellationToken);
    }
  }
}

using GAIA.Core.Documents.Interfaces;
using GAIA.Core.Documents.Mappers;
using GAIA.Core.Documents.Models;
using GAIA.Domain.Documents.Entities;

namespace GAIA.Core.Documents.Services;

public class DocumentService : IDocumentService
{
  private readonly IDocumentRepository _repository;

  public DocumentService(IDocumentRepository repository)
  {
    _repository = repository;
  }

  public async Task<DocumentCollectionDto> GetAllAsync(CancellationToken cancellationToken)
  {
    var documents = await _repository.GetAllAsync(cancellationToken);
    var summaries = documents
      .OrderBy(document => document.Name)
      .Select(document => document.ToSummaryDto())
      .ToList();

    return new DocumentCollectionDto(summaries);
  }

  public async Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    var document = await _repository.GetByIdAsync(id, cancellationToken);
    return document?.ToDto();
  }

  public async Task<Guid> CreateAsync(CreateDocumentDto document, CancellationToken cancellationToken)
  {
    var entity = new Document
    {
      Id = Guid.NewGuid(),
      Content = document.Content,
      Status = document.Status,
      Category = document.Category,
      Name = document.Name
    };

    await _repository.AddAsync(entity, cancellationToken);
    return entity.Id;
  }

  public async Task<bool> UpdateAsync(Guid id, UpdateDocumentDto document, CancellationToken cancellationToken)
  {
    var existing = await _repository.GetByIdAsync(id, cancellationToken);

    if (existing is null)
    {
      return false;
    }

    existing.Status = document.Status;
    existing.Category = document.Category;
    existing.Name = document.Name;

    if (document.Content is not null)
    {
      existing.Content = document.Content;
    }

    await _repository.UpdateAsync(existing, cancellationToken);
    return true;
  }

  public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    var existing = await _repository.GetByIdAsync(id, cancellationToken);

    if (existing is null)
    {
      return false;
    }

    await _repository.DeleteAsync(existing, cancellationToken);
    return true;
  }
}

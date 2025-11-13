using GAIA.Core.Documents.Interfaces;
using GAIA.Domain.Documents.Entities;
using Microsoft.EntityFrameworkCore;

namespace GAIA.Infra.Repositories;

public class DocumentRepository : IDocumentRepository
{
  private readonly ApplicationDbContext _context;

  public DocumentRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _context.Documents
      .AsNoTracking()
      .ToListAsync(cancellationToken);
  }

  public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _context.Documents
      .AsNoTracking()
      .FirstOrDefaultAsync(document => document.Id == id, cancellationToken);
  }

  public async Task AddAsync(Document document, CancellationToken cancellationToken)
  {
    await _context.Documents.AddAsync(document, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task UpdateAsync(Document document, CancellationToken cancellationToken)
  {
    _context.Documents.Update(document);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(Document document, CancellationToken cancellationToken)
  {
    _context.Documents.Remove(document);
    await _context.SaveChangesAsync(cancellationToken);
  }
}

using GAIA.Core.Documents.Interfaces;
using GAIA.Core.Documents.Models;
using GAIA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GAIA.Infra.Repositories
{
  public class DocumentRepository : IDocumentRepository
  {
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken)
    {
      _context.Documents.Add(document);
      await _context.SaveChangesAsync(cancellationToken);
      return document;
    }

    public async Task<IReadOnlyList<DocumentSummary>> GetSummariesAsync(CancellationToken cancellationToken)
    {
      return await _context.Documents
        .AsNoTracking()
        .Select(document => new DocumentSummary(document.Id, document.Status, document.Category, document.Name))
        .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool track = false)
    {
      var query = _context.Documents.AsQueryable();
      if (!track)
      {
        query = query.AsNoTracking();
      }

      return await query.FirstOrDefaultAsync(document => document.Id == id, cancellationToken);
    }

    public async Task<Document?> UpdateAsync(Document document, CancellationToken cancellationToken)
    {
      _context.Documents.Update(document);
      await _context.SaveChangesAsync(cancellationToken);
      return document;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
      var document = await _context.Documents.FindAsync(new object[] { id }, cancellationToken);
      if (document is null)
      {
        return false;
      }

      _context.Documents.Remove(document);
      await _context.SaveChangesAsync(cancellationToken);
      return true;
    }
  }
}

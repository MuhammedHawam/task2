using GAIA.Core.FileStorage.Interfaces;
using GAIA.Domain.FileStorage.Entities;
using Microsoft.EntityFrameworkCore;

namespace GAIA.Infra.EFCore.Repositories;

public class StoredFileRepository : IStoredFileRepository
{
  private readonly GaiaDbContext _context;

  public StoredFileRepository(GaiaDbContext context)
  {
    _context = context;
  }

  public async Task<StoredFile> SaveAsync(StoredFile file, CancellationToken cancellationToken = default)
  {
    if (file is null)
    {
      throw new ArgumentNullException(nameof(file));
    }

    if (file.Id == Guid.Empty)
    {
      file.Id = Guid.NewGuid();
    }

    if (file.CreatedAt == default)
    {
      file.CreatedAt = DateTime.UtcNow;
    }

    _context.StoredFiles.Add(file);
    await _context.SaveChangesAsync(cancellationToken);
    return file;
  }

  public Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return _context.StoredFiles
      .AsNoTracking()
      .FirstOrDefaultAsync(file => file.Id == id, cancellationToken);
  }
}

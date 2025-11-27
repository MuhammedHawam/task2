using GAIA.Domain.FileStorage.Entities;

namespace GAIA.Core.FileStorage.Interfaces;

public interface IStoredFileRepository
{
  Task<StoredFile> SaveAsync(StoredFile file, CancellationToken cancellationToken = default);
  Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

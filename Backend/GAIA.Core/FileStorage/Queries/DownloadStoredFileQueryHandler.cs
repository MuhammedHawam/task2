using GAIA.Core.FileStorage.Interfaces;
using MediatR;

namespace GAIA.Core.FileStorage.Queries;

public class DownloadStoredFileQueryHandler : IRequestHandler<DownloadStoredFileQuery, StoredFileDownload?>
{
  private readonly IStoredFileRepository _storedFileRepository;

  public DownloadStoredFileQueryHandler(IStoredFileRepository storedFileRepository)
  {
    _storedFileRepository = storedFileRepository;
  }

  public async Task<StoredFileDownload?> Handle(DownloadStoredFileQuery request, CancellationToken cancellationToken)
  {
    var storedFile = await _storedFileRepository.GetByIdAsync(request.FileId, cancellationToken);
    if (storedFile is null)
    {
      return null;
    }

    return new StoredFileDownload(
      storedFile.Id,
      storedFile.FileName,
      storedFile.ContentType,
      storedFile.SizeInBytes,
      storedFile.Content
    );
  }
}

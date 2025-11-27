using MediatR;

namespace GAIA.Core.FileStorage.Queries;

public record DownloadStoredFileQuery(Guid FileId) : IRequest<StoredFileDownload?>;

public record StoredFileDownload(
  Guid FileId,
  string FileName,
  string ContentType,
  long SizeInBytes,
  byte[] Content
);

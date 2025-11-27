using GAIA.Domain;

namespace GAIA.Domain.FileStorage.Entities;

public class StoredFile : IdentityObject
{
  public string FileName { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long SizeInBytes { get; set; }
  public string? Description { get; set; }
  public Guid CreatedBy { get; set; }
  public DateTime CreatedAt { get; set; }
  public byte[] Content { get; set; } = Array.Empty<byte>();
}

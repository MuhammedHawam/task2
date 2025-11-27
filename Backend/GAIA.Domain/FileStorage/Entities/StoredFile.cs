namespace GAIA.Domain.FileStorage.Entities;

public class StoredFile
{
  public Guid Id { get; set; }
  public string FileName { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long SizeInBytes { get; set; }
  public string Category { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Guid? ContextId { get; set; }
  public string? ContextType { get; set; }
  public Guid CreatedBy { get; set; }
  public DateTime CreatedAt { get; set; }
  public byte[] Content { get; set; } = Array.Empty<byte>();
}

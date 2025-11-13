using System;

namespace GAIA.Domain.Entities
{
  public class Document
  {
    public Guid Id { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
  }
}

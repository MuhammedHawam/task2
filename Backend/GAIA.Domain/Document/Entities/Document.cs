namespace GAIA.Domain.Document.Entities;

public class Document
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string? Content { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}

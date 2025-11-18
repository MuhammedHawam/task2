using GAIA.Core;

namespace GAIA.Domain.Entities
{
  public class Document : IdentityObject
  {
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
  }
}

using System.ComponentModel.DataAnnotations;

namespace GAIA.Api.Contracts.Documents
{
  public sealed record UpdateDocumentRequest(
    [property: Required, MaxLength(256)] string Name,
    [property: Required, MaxLength(128)] string Category,
    [property: Required, MaxLength(128)] string Status,
    string? ContentBase64);
}

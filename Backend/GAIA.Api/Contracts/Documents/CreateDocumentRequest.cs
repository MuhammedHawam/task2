using System.ComponentModel.DataAnnotations;

namespace GAIA.Api.Contracts.Documents
{
  public sealed record CreateDocumentRequest(
    [property: Required, MaxLength(256)] string Name,
    [property: Required, MaxLength(128)] string Category,
    [property: Required, MaxLength(128)] string Status,
    [property: Required] string ContentBase64);
}

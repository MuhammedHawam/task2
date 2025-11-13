using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GAIA.Api.Contracts.Documents
{
  public sealed class UploadDocumentRequest
  {
    [Required]
    public IFormFile File { get; init; } = default!;

    [Required, MaxLength(256)]
    public string Name { get; init; } = string.Empty;

    [Required, MaxLength(128)]
    public string Category { get; init; } = string.Empty;

    [Required, MaxLength(128)]
    public string Status { get; init; } = string.Empty;
  }
}

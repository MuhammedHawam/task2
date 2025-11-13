using Microsoft.AspNetCore.Http;

namespace GAIA.Api.Contracts.Documents;

public sealed class UploadDocumentRequest
{
  public IFormFile File { get; set; } = default!;
  public string Status { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
}

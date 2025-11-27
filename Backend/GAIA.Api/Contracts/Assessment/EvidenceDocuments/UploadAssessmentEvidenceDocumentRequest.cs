using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GAIA.Api.Contracts.Assessment.EvidenceDocuments;

public class UploadAssessmentEvidenceDocumentRequest
{
  [Required]
  public IFormFile File { get; init; } = default!;

  [Required]
  public Guid UploadedBy { get; init; }

  [MaxLength(128)]
  public string Category { get; init; } = "Evidence";

  [MaxLength(512)]
  public string? Description { get; init; }
}

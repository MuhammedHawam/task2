namespace GAIA.Domain.Assessment.DomainEvents;

public class AssessmentEvidenceDocumentAdded
{
  public Guid AssessmentId { get; set; }
  public Guid EvidenceDocumentId { get; set; }
  public Guid FileId { get; set; }
  public string FileName { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long SizeInBytes { get; set; }
  public string Category { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Guid UploadedBy { get; set; }
  public DateTime UploadedAt { get; set; }
}

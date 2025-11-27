using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Domain.Assessment.Entities;

public class Assessment
{
  public Guid Id { get; set; } // Primary Key
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public Guid CreatedBy { get; set; }
  public Guid FrameworkId { get; set; } // Foreign Key to Framework
  public Guid AssessmentDepthId { get; set; }
  public Guid AssessmentScoringId { get; set; }
  public List<AssessmentEvidenceDocument> EvidenceDocuments { get; set; } = new();


  // Apply method for domain events
  public void Apply(AssessmentCreated e)
  {
    Id = e.Id;
    Title = e.Title;
    Description = e.Description;
    CreatedAt = e.CreatedAt;
    CreatedBy = e.CreatedBy;
    FrameworkId = e.FrameworkId;
    AssessmentDepthId = e.AssessmentDepthId;
    AssessmentScoringId = e.AssessmentScoringId;
  }

  public void Apply(AssessmentEvidenceDocumentAdded e)
  {
    EvidenceDocuments ??= new List<AssessmentEvidenceDocument>();

    var document = new AssessmentEvidenceDocument
    {
      Id = e.EvidenceDocumentId,
      FileId = e.FileId,
      FileName = e.FileName,
      ContentType = e.ContentType,
      SizeInBytes = e.SizeInBytes,
      Category = e.Category,
      Description = e.Description,
      UploadedBy = e.UploadedBy,
      UploadedAt = e.UploadedAt
    };

    var existingIndex = EvidenceDocuments.FindIndex(doc => doc.Id == e.EvidenceDocumentId);
    if (existingIndex >= 0)
    {
      EvidenceDocuments[existingIndex] = document;
    }
    else
    {
      EvidenceDocuments.Add(document);
    }
  }
}

using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.FileStorage.Interfaces;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Documents;

public class AddEvidenceDocumentCommandHandler
  : IRequestHandler<AddEvidenceDocumentCommand, AddEvidenceDocumentResult?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IStoredFileRepository _storedFileRepository;
  private readonly IAssessmentEventWriter _eventWriter;

  public AddEvidenceDocumentCommandHandler(
    IAssessmentRepository assessmentRepository,
    IStoredFileRepository storedFileRepository,
    IAssessmentEventWriter eventWriter)
  {
    _assessmentRepository = assessmentRepository;
    _storedFileRepository = storedFileRepository;
    _eventWriter = eventWriter;
  }

  public async Task<AddEvidenceDocumentResult?> Handle(AddEvidenceDocumentCommand request, CancellationToken cancellationToken)
  {
    if (request.Content is null || request.Content.Length == 0 || request.FileSize <= 0)
    {
      throw new ArgumentException("Evidence document payload must include binary content.", nameof(request));
    }

    var assessment = await _assessmentRepository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var category = string.IsNullOrWhiteSpace(request.Category)
      ? "Evidence"
      : request.Category.Trim();

    var resolvedSize = Math.Max(request.FileSize, request.Content.LongLength);

    var storedFile = new Domain.FileStorage.Entities.StoredFile
    {
      FileName = string.IsNullOrWhiteSpace(request.FileName) ? "evidence-document" : request.FileName,
      ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType,
      SizeInBytes = resolvedSize,
      Description = request.Description,
      CreatedBy = request.UploadedBy,
      Content = request.Content
    };

    var savedFile = await _storedFileRepository.SaveAsync(storedFile, cancellationToken);
    var uploadedAt = savedFile.CreatedAt == default ? DateTime.UtcNow : savedFile.CreatedAt;

    var evidenceDocumentId = Guid.NewGuid();
    var evidenceEvent = new Domain.Assessment.DomainEvents.AssessmentEvidenceDocumentAdded
    {
      AssessmentId = request.AssessmentId,
      EvidenceDocumentId = evidenceDocumentId,
      FileId = savedFile.Id,
      FileName = savedFile.FileName,
      ContentType = savedFile.ContentType,
      SizeInBytes = savedFile.SizeInBytes,
      Category = category,
      Description = request.Description,
      UploadedAt = uploadedAt,
      UploadedBy = request.UploadedBy
    };

    await _eventWriter.AppendAsync(request.AssessmentId, evidenceEvent, cancellationToken);

    return new AddEvidenceDocumentResult(
      evidenceEvent.AssessmentId,
      evidenceEvent.EvidenceDocumentId,
      evidenceEvent.FileId,
      evidenceEvent.FileName,
      evidenceEvent.ContentType,
      resolvedSize,
      category,
      request.Description,
      uploadedAt,
      request.UploadedBy
    );
  }
}


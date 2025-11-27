using System;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.FileStorage.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.FileStorage.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Documents;

public class AddEvidenceDocumentCommandHandler
  : IRequestHandler<AddEvidenceDocumentCommand, AddEvidenceDocumentResult?>
{
  private const string AssessmentEvidenceContext = "AssessmentEvidence";
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

    var storedFile = new StoredFile
    {
      FileName = string.IsNullOrWhiteSpace(request.FileName) ? "evidence-document" : request.FileName,
      ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType,
      SizeInBytes = resolvedSize,
      Category = category,
      Description = request.Description,
      ContextId = request.AssessmentId,
      ContextType = AssessmentEvidenceContext,
      CreatedBy = request.UploadedBy,
      Content = request.Content
    };

    var savedFile = await _storedFileRepository.SaveAsync(storedFile, cancellationToken);

    var evidenceDocumentId = Guid.NewGuid();
    var evidenceEvent = new AssessmentEvidenceDocumentAdded
    {
      AssessmentId = request.AssessmentId,
      EvidenceDocumentId = evidenceDocumentId,
      FileId = savedFile.Id,
      FileName = savedFile.FileName,
      ContentType = savedFile.ContentType,
      SizeInBytes = savedFile.SizeInBytes,
      Category = savedFile.Category,
      Description = savedFile.Description,
      UploadedAt = savedFile.CreatedAt,
      UploadedBy = savedFile.CreatedBy
    };

    await _eventWriter.AppendEvidenceDocumentAsync(evidenceEvent, cancellationToken);

    return new AddEvidenceDocumentResult(
      evidenceEvent.AssessmentId,
      evidenceEvent.EvidenceDocumentId,
      evidenceEvent.FileId,
      evidenceEvent.FileName,
      evidenceEvent.ContentType,
      resolvedSize,
      evidenceEvent.Category,
      evidenceEvent.Description,
      evidenceEvent.UploadedAt,
      evidenceEvent.UploadedBy
    );
  }
}

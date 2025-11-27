using System.Linq;
using GAIA.Core.Assessment.Interfaces;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentDocumentsQueryHandler
  : IRequestHandler<GetAssessmentDocumentsQuery, AssessmentDocumentsResult?>
{
  private readonly IAssessmentRepository _assessmentRepository;

  public GetAssessmentDocumentsQueryHandler(IAssessmentRepository assessmentRepository)
  {
    _assessmentRepository = assessmentRepository;
  }

  public async Task<AssessmentDocumentsResult?> Handle(GetAssessmentDocumentsQuery request, CancellationToken cancellationToken)
  {
    var assessment = await _assessmentRepository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var documents = assessment.EvidenceDocuments
      .OrderByDescending(doc => doc.UploadedAt)
      .Select(doc => new AssessmentEvidenceDocumentModel(
        doc.Id,
        doc.FileId,
        doc.FileName,
        doc.ContentType,
        doc.SizeInBytes,
        doc.Category,
        doc.Description,
        doc.UploadedBy,
        doc.UploadedAt
      ))
      .ToList();

    return new AssessmentDocumentsResult(request.AssessmentId, documents);
  }
}

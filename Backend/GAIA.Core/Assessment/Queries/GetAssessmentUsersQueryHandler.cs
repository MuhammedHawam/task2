using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentUsersQueryHandler
  : IRequestHandler<GetAssessmentUsersQuery, AssessmentUserAssignment?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IAssessmentUserAssignmentRepository _assignmentRepository;

  public GetAssessmentUsersQueryHandler(
    IAssessmentRepository assessmentRepository,
    IAssessmentUserAssignmentRepository assignmentRepository)
  {
    _assessmentRepository = assessmentRepository;
    _assignmentRepository = assignmentRepository;
  }

  public async Task<AssessmentUserAssignment?> Handle(
    GetAssessmentUsersQuery request,
    CancellationToken cancellationToken)
  {
    var assessment = await _assessmentRepository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var assignment = await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken);

    return assignment ?? new AssessmentUserAssignment(request.AssessmentId);
  }
}

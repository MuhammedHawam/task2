using System;
using System.Linq;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class RemoveAssessmentUsersCommandHandler
  : IRequestHandler<RemoveAssessmentUsersCommand, AssessmentUserAssignment?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IAssessmentUserAssignmentRepository _assignmentRepository;
  private readonly IAssessmentUserAssignmentEventWriter _eventWriter;

  public RemoveAssessmentUsersCommandHandler(
    IAssessmentRepository assessmentRepository,
    IAssessmentUserAssignmentRepository assignmentRepository,
    IAssessmentUserAssignmentEventWriter eventWriter)
  {
    _assessmentRepository = assessmentRepository;
    _assignmentRepository = assignmentRepository;
    _eventWriter = eventWriter;
  }

  public async Task<AssessmentUserAssignment?> Handle(
    RemoveAssessmentUsersCommand request,
    CancellationToken cancellationToken)
  {
    var assessment = await _assessmentRepository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var assignment = await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken);
    if (assignment is null)
    {
      return null;
    }

    var userIds = request.UserIds ?? Array.Empty<Guid>();
    if (!userIds.Any())
    {
      return assignment;
    }

    var removedEvent = new AssessmentUsersRemoved
    {
      AssessmentId = request.AssessmentId,
      UserIds = userIds.ToList(),
      RemovedAt = DateTime.UtcNow
    };

    await _eventWriter.RemoveAsync(removedEvent, cancellationToken);

    return await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken) ??
           new AssessmentUserAssignment(request.AssessmentId);
  }
}

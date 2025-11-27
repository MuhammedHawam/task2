using System;
using System.Linq;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class UpdateAssessmentUsersCommandHandler
  : IRequestHandler<UpdateAssessmentUsersCommand, AssessmentUserAssignment?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IAssessmentUserAssignmentRepository _assignmentRepository;
  private readonly IAssessmentUserAssignmentEventWriter _eventWriter;

  public UpdateAssessmentUsersCommandHandler(
    IAssessmentRepository assessmentRepository,
    IAssessmentUserAssignmentRepository assignmentRepository,
    IAssessmentUserAssignmentEventWriter eventWriter)
  {
    _assessmentRepository = assessmentRepository;
    _assignmentRepository = assignmentRepository;
    _eventWriter = eventWriter;
  }

  public async Task<AssessmentUserAssignment?> Handle(
    UpdateAssessmentUsersCommand request,
    CancellationToken cancellationToken)
  {
    var assessment = await _assessmentRepository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var incomingUsers = request.Users ?? Array.Empty<AssessmentUserInput>();

    var snapshotUsers = incomingUsers
      .Select(user => new AssessmentUserSnapshot
      {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        Avatar = user.Avatar,
        Role = user.Role
      })
      .ToList();

    var updatedEvent = new AssessmentUsersUpdated
    {
      AssessmentId = request.AssessmentId,
      Users = snapshotUsers,
      UpdatedAt = DateTime.UtcNow
    };

    await _eventWriter.UpdateAsync(updatedEvent, cancellationToken);

    return await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken) ??
           new AssessmentUserAssignment(request.AssessmentId);
  }
}

using System;
using System.Linq;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class AssignUsersToAssessmentCommandHandler
  : IRequestHandler<AssignUsersToAssessmentCommand, AssessmentUserAssignment?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IAssessmentUserAssignmentRepository _assignmentRepository;
  private readonly IAssessmentUserAssignmentEventWriter _eventWriter;

  public AssignUsersToAssessmentCommandHandler(
    IAssessmentRepository assessmentRepository,
    IAssessmentUserAssignmentRepository assignmentRepository,
    IAssessmentUserAssignmentEventWriter eventWriter)
  {
    _assessmentRepository = assessmentRepository;
    _assignmentRepository = assignmentRepository;
    _eventWriter = eventWriter;
  }

  public async Task<AssessmentUserAssignment?> Handle(
    AssignUsersToAssessmentCommand request,
    CancellationToken cancellationToken)
  {
    var assessment = await _assessmentRepository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var incomingUsers = request.Users ?? Array.Empty<AssessmentUserInput>();
    if (!incomingUsers.Any())
    {
      return await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken);
    }

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

    var assignedEvent = new AssessmentUsersAssigned
    {
      AssessmentId = request.AssessmentId,
      Users = snapshotUsers,
      AssignedAt = DateTime.UtcNow
    };

    await _eventWriter.AssignAsync(assignedEvent, cancellationToken);

    return await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken) ??
           new AssessmentUserAssignment(request.AssessmentId);
  }
}

using System;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class AssignUsersToAssessmentCommandHandler
  : IRequestHandler<AssignUsersToAssessmentCommand, AssessmentUserAssignment?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IAssessmentUserAssignmentRepository _assignmentRepository;

  public AssignUsersToAssessmentCommandHandler(
    IAssessmentRepository assessmentRepository,
    IAssessmentUserAssignmentRepository assignmentRepository)
  {
    _assessmentRepository = assessmentRepository;
    _assignmentRepository = assignmentRepository;
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

    var assignment =
      await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken) ??
      new AssessmentUserAssignment(request.AssessmentId);

    var incomingUsers = request.Users ?? Array.Empty<AssessmentUserInput>();

    var newUsers = incomingUsers
      .Select(user => new AssessmentAssignedUser
      {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        Avatar = user.Avatar,
        Role = user.Role
      });

    assignment.AddUsers(newUsers);

    await _assignmentRepository.SaveAsync(assignment, cancellationToken);

    return assignment;
  }
}

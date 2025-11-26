using System;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class UpdateAssessmentUsersCommandHandler
  : IRequestHandler<UpdateAssessmentUsersCommand, AssessmentUserAssignment?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IAssessmentUserAssignmentRepository _assignmentRepository;

  public UpdateAssessmentUsersCommandHandler(
    IAssessmentRepository assessmentRepository,
    IAssessmentUserAssignmentRepository assignmentRepository)
  {
    _assessmentRepository = assessmentRepository;
    _assignmentRepository = assignmentRepository;
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

    var assignment =
      await _assignmentRepository.GetAsync(request.AssessmentId, cancellationToken) ??
      new AssessmentUserAssignment(request.AssessmentId);

    var incomingUsers = request.Users ?? Array.Empty<AssessmentUserInput>();

    var mappedUsers = incomingUsers
      .Select(user => new AssessmentAssignedUser
      {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        Avatar = user.Avatar,
        Role = user.Role
      })
      .ToList();

    assignment.ReplaceUsers(mappedUsers);

    await _assignmentRepository.SaveAsync(assignment, cancellationToken);

    return assignment;
  }
}

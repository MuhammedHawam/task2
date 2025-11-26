namespace GAIA.Core.Assessment.Commands.Assessment;

public record AssessmentUserInput(
  Guid UserId,
  string Username,
  string Email,
  string? Avatar,
  string Role
);

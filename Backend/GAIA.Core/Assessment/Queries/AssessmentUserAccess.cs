namespace GAIA.Core.Assessment.Queries;

public record AssessmentUserAccess(
  Guid Id,
  string Username,
  string Email,
  string? Avatar,
  string Role
);

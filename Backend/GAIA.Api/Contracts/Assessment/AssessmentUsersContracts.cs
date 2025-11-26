namespace GAIA.Api.Contracts.Assessment;

public record AssessmentUsersRequest(IReadOnlyList<AssessmentUserRequest> Users);

public record AssessmentUserRequest(
  Guid Id,
  string Username,
  string Email,
  string? Avatar,
  string Role
);

public record AssessmentUsersResponse(
  Guid AssessmentId,
  IReadOnlyList<UserResponse> Users
);

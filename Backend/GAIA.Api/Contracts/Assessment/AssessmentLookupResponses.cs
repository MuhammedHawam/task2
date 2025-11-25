namespace GAIA.Api.Contracts.Assessment;

public record OrganizationListResponse(IReadOnlyList<OrganizationResponse> Organizations);

public record OrganizationResponse(
  Guid Id,
  string Name,
  string? LogoUrl,
  string? WebsiteUrl,
  string? Description
);

public record LanguageListResponse(IReadOnlyList<string> Languages);

public record UserAccessListResponse(IReadOnlyList<UserResponse> Users);

public record UserResponse(
  Guid Id,
  string Username,
  string Email,
  string? Avatar,
  string Role
);

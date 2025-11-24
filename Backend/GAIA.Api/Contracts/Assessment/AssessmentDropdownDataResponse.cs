namespace GAIA.Api.Contracts.Assessment;

public record AssessmentDropdownDataResponse(
  IReadOnlyList<OrganizationResponse> Organizations,
  IReadOnlyList<string> Languages,
  IReadOnlyList<DropdownOptionResponse> Statuses,
  IReadOnlyList<DropdownOptionResponse> IconTypes,
  IReadOnlyList<DropdownOptionResponse> RoleTypes
);

public record OrganizationResponse(
  Guid Id,
  string Name,
  string? LogoUrl,
  string? WebsiteUrl,
  string? Description
);

public record DropdownOptionResponse(
  string Code,
  string Label
);

namespace GAIA.Core.Assessment.Queries;

public record AssessmentOrganization(
  Guid Id,
  string Name,
  string? LogoUrl,
  string? WebsiteUrl,
  string? Description
);

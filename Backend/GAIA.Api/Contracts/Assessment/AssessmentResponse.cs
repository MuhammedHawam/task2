namespace GAIA.Api.Contracts.Assessment;

public record AssessmentResponse(
  Guid Id,
  string Name,
  DateTime StartDate,
  DateTime EndDate,
  Guid OrganizationId,
  string Organization,
  string Language,
  DateTime CreatedAt,
  DateTime? UpdatedAt
);

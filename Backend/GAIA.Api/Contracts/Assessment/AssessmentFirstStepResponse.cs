namespace GAIA.Api.Contracts.Assessment;

public record AssessmentFirstStepResponse(
  Guid Id,
  string Name,
  DateTime StartDate,
  DateTime EndDate,
  string Organization,
  string Language,
  DateTime CreatedAt,
  DateTime? UpdatedAt
);

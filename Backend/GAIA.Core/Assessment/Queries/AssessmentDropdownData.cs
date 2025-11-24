using System.Collections.ObjectModel;

namespace GAIA.Core.Assessment.Queries;

public record AssessmentDropdownData(
  IReadOnlyList<AssessmentOrganization> Organizations,
  IReadOnlyList<string> Languages,
  IReadOnlyList<AssessmentDropdownOption> Statuses,
  IReadOnlyList<AssessmentDropdownOption> IconTypes,
  IReadOnlyList<AssessmentDropdownOption> RoleTypes
);

public record AssessmentOrganization(
  Guid Id,
  string Name,
  string? LogoUrl,
  string? WebsiteUrl,
  string? Description
);

public record AssessmentDropdownOption(string Code, string Label);

internal static class AssessmentDropdownDefaults
{
  internal static readonly IReadOnlyList<AssessmentDropdownOption> Statuses =
    Array.AsReadOnly(new[]
    {
      new AssessmentDropdownOption("on-schedule", "On Schedule"),
      new AssessmentDropdownOption("behind-schedule", "Behind Schedule"),
      new AssessmentDropdownOption("behind-schedule-overdue", "Behind Schedule & Overdue"),
      new AssessmentDropdownOption("complete", "Complete")
    });

  internal static readonly IReadOnlyList<AssessmentDropdownOption> IconTypes =
    Array.AsReadOnly(new[]
    {
      new AssessmentDropdownOption("team", "Team"),
      new AssessmentDropdownOption("organization", "Organization"),
      new AssessmentDropdownOption("deadline", "Deadline"),
      new AssessmentDropdownOption("update", "Update"),
      new AssessmentDropdownOption("task", "Task")
    });

  internal static readonly IReadOnlyList<AssessmentDropdownOption> RoleTypes =
    Array.AsReadOnly(new[]
    {
      new AssessmentDropdownOption("Admin", "Admin"),
      new AssessmentDropdownOption("Accessor", "Accessor"),
      new AssessmentDropdownOption("Editor", "Editor"),
      new AssessmentDropdownOption("Viewer", "Viewer")
    });
}

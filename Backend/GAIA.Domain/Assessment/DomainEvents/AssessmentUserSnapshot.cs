using System;

namespace GAIA.Domain.Assessment.DomainEvents;

public class AssessmentUserSnapshot
{
  public Guid UserId { get; set; }
  public string Username { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string? Avatar { get; set; }
  public string Role { get; set; } = string.Empty;
}

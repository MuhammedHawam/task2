using System;
using System.Collections.Generic;
using System.Linq;

namespace GAIA.Domain.Assessment.Entities;

public class AssessmentUserAssignment
{
  public Guid Id { get; set; }

  public Guid AssessmentId => Id;

  public List<AssessmentAssignedUser> Users { get; set; } = new();

  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  public AssessmentUserAssignment()
  {
  }

  public AssessmentUserAssignment(Guid assessmentId)
  {
    Id = assessmentId;
  }

  public void AddUsers(IEnumerable<AssessmentAssignedUser> users)
  {
    if (users is null)
    {
      return;
    }

    var existingIds = new HashSet<Guid>(Users.Select(user => user.UserId));
    foreach (var user in users)
    {
      if (existingIds.Add(user.UserId))
      {
        Users.Add(user);
      }
    }

    UpdatedAt = DateTime.UtcNow;
  }

  public void ReplaceUsers(IEnumerable<AssessmentAssignedUser> users)
  {
    Users = users?
      .GroupBy(user => user.UserId)
      .Select(group => group.First())
      .ToList()
      ?? new List<AssessmentAssignedUser>();

    UpdatedAt = DateTime.UtcNow;
  }
}

public class AssessmentAssignedUser
{
  public Guid UserId { get; set; }
  public string Username { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string? Avatar { get; set; }
  public string Role { get; set; } = string.Empty;
}

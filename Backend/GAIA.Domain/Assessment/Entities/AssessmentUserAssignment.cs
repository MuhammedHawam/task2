using System;
using System.Collections.Generic;
using System.Linq;
using GAIA.Domain.Assessment.DomainEvents;

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

  public void RemoveUsers(IEnumerable<Guid> userIds)
  {
    if (userIds is null)
    {
      return;
    }

    var ids = new HashSet<Guid>(userIds);
    Users = Users
      .Where(user => !ids.Contains(user.UserId))
      .ToList();
    UpdatedAt = DateTime.UtcNow;
  }

  public void Apply(AssessmentUsersAssigned @event)
  {
    if (Id == Guid.Empty)
    {
      Id = @event.AssessmentId;
    }

    var users = @event.Users?
      .Select(user => new AssessmentAssignedUser
      {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        Avatar = user.Avatar,
        Role = user.Role
      });

    AddUsers(users ?? Array.Empty<AssessmentAssignedUser>());
  }

  public void Apply(AssessmentUsersUpdated @event)
  {
    if (Id == Guid.Empty)
    {
      Id = @event.AssessmentId;
    }

    var users = @event.Users?
      .Select(user => new AssessmentAssignedUser
      {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        Avatar = user.Avatar,
        Role = user.Role
      })
      .ToList();

    ReplaceUsers(users ?? new List<AssessmentAssignedUser>());
  }

  public void Apply(AssessmentUsersRemoved @event)
  {
    if (Id == Guid.Empty)
    {
      Id = @event.AssessmentId;
    }

    RemoveUsers(@event.UserIds);
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

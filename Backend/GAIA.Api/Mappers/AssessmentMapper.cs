using GAIA.Api.Contracts.Assessment;
using GAIA.Domain.Assessment.Entities;
using System.Linq;

namespace GAIA.Api.Mappers
{
  public static class AssessmentMapper
  {
    public static AssessmentDetailsResponse ToResponse(this Core.Assessment.Queries.AssessmentDetails details) => new(
      details.Assessment.Id,
      details.Assessment.AssessmentId,
      details.Assessment.Title,
      details.Assessment.Description,
      details.Assessment.CreatedAt,
      details.Assessment.CreatedBy,
      details.Assessment.FrameworkId,
      details.Assessment.AssessmentDepthId,
      details.Assessment.AssessmentScoringId,
      details.Depth?.ToResponse(),
      details.Scoring?.ToResponse()
    );

    private static AssessmentDepthResponse ToResponse(this AssessmentDepth depth) => new(
      depth.Id,
      depth.FrameworkId,
      depth.Name,
      depth.Depth
    );

    private static AssessmentScoringResponse ToResponse(this AssessmentScoring scoring) => new(
      scoring.Id,
      scoring.FrameworkId,
      scoring.Name,
      scoring.Description
    );

    public static AssessmentResponse ToResponse(this Assessment assessment) => new(
      assessment.Id,
      assessment.Name,
      assessment.StartDate,
      assessment.EndDate,
      assessment.OrganizationId,
      assessment.Organization,
      assessment.Language,
      assessment.CreatedAt,
      assessment.UpdatedAt
    );

    public static OrganizationListResponse ToResponse(this IReadOnlyList<Core.Assessment.Queries.AssessmentOrganization> data) =>
      new(
        data
          .Select(org => new OrganizationResponse(org.Id, org.Name, org.LogoUrl, org.WebsiteUrl, org.Description))
          .ToList()
      );

    public static LanguageListResponse ToResponse(this IReadOnlyList<string> languages) =>
      new(languages);

    public static UserAccessListResponse ToResponse(this IReadOnlyList<Core.Assessment.Queries.AssessmentUserAccess> users) =>
      new(
        users
          .Select(user => new UserResponse(user.Id, user.Username, user.Email, user.Avatar, user.Role))
          .ToList()
      );

    public static AssessmentUsersResponse ToResponse(this AssessmentUserAssignment assignment) => new(
      assignment.AssessmentId,
      assignment.Users
        .Select(user => new UserResponse(user.UserId, user.Username, user.Email, user.Avatar, user.Role))
        .ToList()
    );
  }
}

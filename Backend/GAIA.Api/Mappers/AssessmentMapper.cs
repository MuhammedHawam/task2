using GAIA.Api.Contracts.Assessment;
using GAIA.Domain.Assessment.Entities;
using System.Linq;

namespace GAIA.Api.Mappers
{
  public static class AssessmentMapper
  {
    public static AssessmentDetailsResponse ToResponse(this Core.Assessment.Queries.AssessmentDetails details) => new(
    details.Assessment.Id,
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
      assessment.Organization,
      assessment.Language,
      assessment.CreatedAt,
      assessment.UpdatedAt
    );

    public static AssessmentDropdownDataResponse ToResponse(this Core.Assessment.Queries.AssessmentDropdownData data) =>
      new(
        data.Organizations
          .Select(org => new OrganizationResponse(org.Id, org.Name, org.LogoUrl, org.WebsiteUrl, org.Description))
          .ToList(),
        data.Languages.ToList(),
        data.Statuses.Select(option => new DropdownOptionResponse(option.Code, option.Label)).ToList(),
        data.IconTypes.Select(option => new DropdownOptionResponse(option.Code, option.Label)).ToList(),
        data.RoleTypes.Select(option => new DropdownOptionResponse(option.Code, option.Label)).ToList()
      );
  }
}

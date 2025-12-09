using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PIF.EBP.Application.Comments.DTOs;
using PIF.EBP.Core.Comments;
using PIF.EBP.Core.Comments.DTOs;

namespace PIF.EBP.Application.Comments.Implementation
{
    public class ProjectOverviewCommentsAppService : IProjectOverviewCommentsAppService
    {
        private readonly IProjectOverviewCommentsIntegrationService _commentsIntegrationService;

        public ProjectOverviewCommentsAppService(IProjectOverviewCommentsIntegrationService commentsIntegrationService)
        {
            _commentsIntegrationService = commentsIntegrationService ?? throw new ArgumentNullException(nameof(commentsIntegrationService));
        }

        public async Task<ProjectOverviewCommentsResponseDto> GetProjectOverviewCommentsAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default)
        {
            var response = await _commentsIntegrationService
                .GetProjectOverviewCommentsAsync(projectOverviewId, page, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var comments = response?.Items?.Select(MapToDto).Where(c => c != null).ToList()
                           ?? new List<ProjectOverviewCommentDto>();

            return new ProjectOverviewCommentsResponseDto
            {
                Page = response?.Page ?? page,
                PageSize = response?.PageSize ?? pageSize,
                TotalCount = response?.TotalCount ?? comments.Count,
                Comments = comments
            };
        }

        public async Task<ProjectOverviewCommentDto> CreateProjectOverviewCommentAsync(
            CreateProjectOverviewCommentDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var integrationRequest = new ProjectOverviewCommentCreateRequest
            {
                ProjectOverviewId = request.ProjectOverviewId,
                FieldId = request.FieldId?.Trim(),
                Comment = request.Comment?.Trim()
            };

            var response = await _commentsIntegrationService
                .CreateProjectOverviewCommentAsync(integrationRequest, cancellationToken)
                .ConfigureAwait(false);

            return MapToDto(response);
        }

        private static ProjectOverviewCommentDto MapToDto(ProjectOverviewCommentItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new ProjectOverviewCommentDto
            {
                Id = item.Id,
                Comment = item.Comment,
                FieldId = item.FieldId,
                ProjectOverviewId = item.ProjectOverviewId ?? 0,
                CreatedBy = item.Creator?.Name,
                DateCreated = item.DateCreated ?? item.DateModified
            };
        }
    }
}

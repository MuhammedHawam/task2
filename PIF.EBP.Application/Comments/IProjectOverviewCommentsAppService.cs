using System.Threading;
using System.Threading.Tasks;
using PIF.EBP.Application.Comments.DTOs;
using PIF.EBP.Core.DependencyInjection;

namespace PIF.EBP.Application.Comments
{
    public interface IProjectOverviewCommentsAppService : ITransientDependency
    {
        Task<ProjectOverviewCommentsResponseDto> GetProjectOverviewCommentsAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default);

        Task<ProjectOverviewCommentDto> CreateProjectOverviewCommentAsync(
            CreateProjectOverviewCommentDto request,
            CancellationToken cancellationToken = default);
    }
}

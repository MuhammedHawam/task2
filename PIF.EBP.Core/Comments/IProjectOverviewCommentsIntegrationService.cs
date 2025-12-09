using System.Threading;
using System.Threading.Tasks;
using PIF.EBP.Core.Comments.DTOs;
using PIF.EBP.Core.DependencyInjection;

namespace PIF.EBP.Core.Comments
{
    public interface IProjectOverviewCommentsIntegrationService : ITransientDependency
    {
        Task<ProjectOverviewCommentsCollectionResponse> GetProjectOverviewCommentsAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default);

        Task<ProjectOverviewCommentItem> CreateProjectOverviewCommentAsync(
            ProjectOverviewCommentCreateRequest request,
            CancellationToken cancellationToken = default);
    }
}

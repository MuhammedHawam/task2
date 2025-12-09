using PIF.EBP.Application.Comments;
using PIF.EBP.Application.Comments.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers.Comments
{
    [ApiResponseWrapper]
    [RoutePrefix("comments/project-overview")]
    public class ProjectOverviewCommentsController : ApiController
    {
        private readonly IProjectOverviewCommentsAppService _commentsAppService;

        public ProjectOverviewCommentsController()
        {
            _commentsAppService = WindsorContainerProvider.Container.Resolve<IProjectOverviewCommentsAppService>();
        }

        [HttpGet]
        [Route("{projectOverviewId:long}")]
        public async Task<IHttpActionResult> GetComments(long projectOverviewId, int page = 1, int pageSize = 1000)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero.");
            }

            if (page <= 0)
            {
                return BadRequest("Page must be greater than zero.");
            }

            if (pageSize <= 0 || pageSize > 1000)
            {
                return BadRequest("Page size must be between 1 and 1000.");
            }

            try
            {
                var comments = await _commentsAppService.GetProjectOverviewCommentsAsync(projectOverviewId, page, pageSize);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateComment([FromBody] CreateProjectOverviewCommentDto request)
        {
            if (request == null)
            {
                return BadRequest("Request payload is required.");
            }

            if (request.ProjectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(request.FieldId))
            {
                return BadRequest("FieldId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return BadRequest("Comment is required.");
            }

            try
            {
                var createdComment = await _commentsAppService.CreateProjectOverviewCommentAsync(request);
                return Ok(createdComment);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}

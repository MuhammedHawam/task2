using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PIF.EBP.Application.Comments.DTOs
{
    public class ProjectOverviewCommentDto
    {
        public long Id { get; set; }

        public string Comment { get; set; }

        public string FieldId { get; set; }

        public long ProjectOverviewId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? DateCreated { get; set; }
    }

    public class CreateProjectOverviewCommentDto
    {
        [Range(1, long.MaxValue, ErrorMessage = "Project overview ID must be greater than zero.")]
        public long ProjectOverviewId { get; set; }

        [Required]
        public string FieldId { get; set; }

        [Required]
        public string Comment { get; set; }
    }

    public class ProjectOverviewCommentsResponseDto
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public IReadOnlyList<ProjectOverviewCommentDto> Comments { get; set; } = Array.Empty<ProjectOverviewCommentDto>();
    }
}

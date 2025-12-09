using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PIF.EBP.Core.Comments.DTOs
{
    public class ProjectOverviewCommentsCollectionResponse
    {
        [JsonProperty("items")]
        public List<ProjectOverviewCommentItem> Items { get; set; } = new List<ProjectOverviewCommentItem>();

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }
    }

    public class ProjectOverviewCommentItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("fieldId")]
        public string FieldId { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public DateTime? DateModified { get; set; }

        [JsonProperty("creator")]
        public ProjectOverviewCommentCreator Creator { get; set; }

        [JsonProperty("r_relationProjectOverviewComment_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }

        [JsonProperty("relationProjectOverviewCommentERC")]
        public string RelationProjectOverviewCommentExternalReferenceCode { get; set; }
    }

    public class ProjectOverviewCommentCreator
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ProjectOverviewCommentCreateRequest
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("fieldId")]
        public string FieldId { get; set; }

        [JsonProperty("r_relationProjectOverviewComment_c_grtProjectOverviewId")]
        public long ProjectOverviewId { get; set; }
    }
}

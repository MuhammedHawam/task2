namespace PIF.EBP.Application.Shared.AppRequest
{
    public class PagingRequest
    {
        public int PageNo { get; set; } = 1;
        private int pageSize = 10;
        public int PageSize
        {
            get => pageSize <= 0 ? 10 : pageSize;
            set => pageSize = value;
        }
        public string SortField { get; set; }
        public Enums.SortOrder SortOrder { get; set; }
    }
    public class FieldsFilters
    {
        public Enums.MatchMode MatchMode { get; set; }
        public Enums.Operator Operator { get; set; }
        public string Value { get; set; }
        public int FieldType { get; set; }
        public string FieldName { get; set; }
    }
}

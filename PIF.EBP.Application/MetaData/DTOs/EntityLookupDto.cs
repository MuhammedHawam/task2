using System;

namespace PIF.EBP.Application.MetaData.DTOs
{
    public class EntityLookupDto
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }

    }
    public class EntityLookupOptions
    {
        public string EntityName { get; set; }
        public string AttributeName { get; set; }
        public string EnColumn { get; set; }
        public string ArColumn { get; set; }
        public string Value { get; set; }
        public string CompanyColumn { get; set; }
        public string ContactColumn { get; set; }
        public Guid? ViewId { get; set; }
    }
}

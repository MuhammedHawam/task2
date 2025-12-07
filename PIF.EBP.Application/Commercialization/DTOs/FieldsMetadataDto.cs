using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class FieldsMetadataDto
    {
        public string RequestTitle { get; set; }
        public string RequestTitleAr { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceDescriptionAr { get; set; }
        public List<FormPageDto> FormPages { get; set; }
    }

    public class FieldDto
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Label { get; set; }
        public string LabelAr { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Rules { get; set; }
        public bool Required { get; set; }
        public int Order { get; set; }
        public List<OptionDto> Options { get; set; }
        public string Condition { get; set; }
    }

    public class OptionDto
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class FormPageDto
    {
        public string PageTitle { get; set; }
        public string PageTitleAr { get; set; }
        public List<FieldDto> Fields { get; set; }
    }

}

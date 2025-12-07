namespace PIF.EBP.Application.MetaData.DTOs
{
    public class EntityGridViewData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FetchXml { get; set; }
        public string LayoutXml { get; set; }
        public string EntityName { get; internal set; }
    }
}

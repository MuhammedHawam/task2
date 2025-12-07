namespace PIF.EBP.Application.PortalAdministration.DTOs
{
    public class CompanyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string RoleName { get; set; }
        public string RoleNameAr { get; set; }
        public string PortalRoleAssociationId { get; set; }
        public byte[] EntityImage { get; set; }
    }
}

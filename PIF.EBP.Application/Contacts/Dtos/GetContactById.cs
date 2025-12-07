using PIF.EBP.Application.MetaData.DTOs;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class GetContactById
    {
        public string FirstName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastName { get; set; }
        public string LastNameAr { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public EntityOptionSetDto Nationality { get; set; }
        public bool InvitedToPortal { get; set; }
        public int? AssociationStatus { get; set; }
        public EntityReferenceDto Company { get; set; }
        public EntityReferenceDto Country { get; set; }
        public EntityReferenceDto Department { get; set; }
        public EntityReferenceDto Position { get; set; }
        public EntityReferenceDto PortalRole { get; set; }
        public EntityReferenceDto ParentPortalRole { get; set; }
    }
}

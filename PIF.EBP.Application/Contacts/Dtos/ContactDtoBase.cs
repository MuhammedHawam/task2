namespace PIF.EBP.Application.Contacts.Dtos
{
    public abstract class ContactDtoBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string PortalRole { get; set; }
        public int Nationality { get; set; }
        public bool InvitedToPortal { get; set; }
    }
}

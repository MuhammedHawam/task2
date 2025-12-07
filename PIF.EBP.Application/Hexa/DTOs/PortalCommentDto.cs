using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class PortalCommentDto
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public DateTime CreationDate { get; set; }
        public EntityReferenceDto Contact { get; set; }
        public EntityReferenceDto Owner { get; set; }
        public byte[] ContactImage { get; set; }
        public byte[] OwnerImage { get; set; }
        public bool IsExternal { get; set; }

    }
}

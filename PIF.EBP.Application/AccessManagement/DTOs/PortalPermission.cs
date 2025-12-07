using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using System.Collections.Generic;

namespace PIF.EBP.Application.AccessManagement.DTOs
{
    public class PortalPermission : ICacheItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Read { get; set; }
        public int Create { get; set; }
        public int Write { get; set; }
        public int Delete { get; set; }
        public string ServiceId { get; set; }
        public List<string> LinkedRoles { get; set; }

        public EntityReferenceDto PortalPage { get; set; }
    }
}

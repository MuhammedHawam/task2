using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PerformanceDashboard.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Settings.DTOs
{

    public class SynergyUserDataDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public EntityReferenceDto Position { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public SectorDto Sector { get; set; }

    }
    public class SectorDto
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }



}

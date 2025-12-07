using System.Collections.Generic;

namespace PIF.EBP.Application.Lookups.DTOs
{
    public class UserProfileMasterDataDto
    {
        public List<MasterData> Countries { get; set; }
        public List<MasterData> Cities { get; set; }
        public List<MasterData> Degrees { get; set; }
        public List<MasterData> Institutes { get; set; }
        public List<MasterData> Majors { get; set; }
        public List<MasterData> Positions { get; set; }
        public List<MasterData> Experiences { get; set; }
    }
}

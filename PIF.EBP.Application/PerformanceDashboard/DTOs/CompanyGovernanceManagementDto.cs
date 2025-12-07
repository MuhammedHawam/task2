using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.AppRequest;
using System;
using System.Collections.Generic;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.PerformanceDashboard.DTOs
{
    public class CompanyGovernanceManagementDto
    {
        public List<BorderMembers> BorderMembers { get; set; }
        public int BorderMembersCount { get; set; }
        public List<EBPUsers> EBPUsers { get; set; }
        public int EBPUsersCount { get; set; }
        public List<CommitteeMember> CommitteeMembers { get; set; }
        public int CommitteeMembersCount { get; set; }
    }

    public class BorderMembers
    {
        public Guid ContactId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public byte[] EntityImage { get; set; }
        public EntityOptionSetDto Affiliation { get; set; }
        public EntityOptionSetDto Position { get; set; }
        public EntityOptionSetDto Title { get; set; }
        public EntityReferenceDto Department { get; set; }
    }

    public class EBPUsers
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public byte[] EntityImage { get; set; }
        public EntityReferenceDto Position { get; set; }
        public EntityReferenceDto Department { get; set; }
        public EntityReferenceDto Role { get; set; }
        public string ContactDetailsEmail { get; set; }
    }

    public class CommitteeMember
    {
        public Guid ContactId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string CommitteeName { get; set; }
        public string CommitteeNameAr { get; set; }
        public EntityOptionSetDto Title { get; set; }
        public EntityOptionSetDto Position { get; set; }
    }

    public class CompanyGovernanceManagementRequestDto
    {
        public CompanyGovernanceManagementRequestDto()
        {
            if (PagingRequest == null)
            {
                PagingRequest = new PagingRequest();
            }
        }

        public Guid CompanyId { get; set; }
        public int? Scope { get; set; } = (int)CompanyGovernanceManagementScope.All;

        public PagingRequest PagingRequest { get; set; }
    }
}

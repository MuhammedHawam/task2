using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PerformanceDashboard.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Settings.DTOs
{
    public class UserPorfileDto
    {
        public int? Otp { get; set; }
        public int? NationalityId { get; set; }
        public string WorkExperience { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? CityId { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public List<UserProfileEducationDto> Educations { get; set; }
        public List<Guid> EducationDeletedIds { get; set; }
    }
    public class UserProfileImageDto
    {
        public string Image { get; set; }
    }

    public class UserProfileEducationDto
    {
        public Guid Id { get; set; }
        public Guid InstitutsId { get; set; }
        public Guid DegreeId { get; set; }
        public Guid MajorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class UserProfileData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public UserCompany UserCompany { get; set; }
        public EntityOptionSetDto Nationality { get; set; }
        public string WorkExperience { get; set; }
        public EntityReferenceDto Position { get; set; }
        public EntityReferenceDto Country { get; set; }
        public EntityReferenceDto City { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public byte[] Image { get; set; }
        public List<UserProfileEducation> Educations { get; set; } = new List<UserProfileEducation>();

    }
    public class UserProfileEducation
    {
        public Guid Id { get; set; }
        public EntityReferenceDto Instituts { get; set; }
        public EntityReferenceDto Degree { get; set; }
        public EntityReferenceDto Major { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class UserCompany 
    {
        public string Name { get; set; }
        public string NameAR { get; set; }
        public string ValidDomain { get; set; }
        public byte[] Image { get; set; }
    }

    public class UpdateEmailRequest
    {
        public string ContactId { get; set; }
        public string NewEmail { get; set; }
    }
}

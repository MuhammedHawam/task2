using System.Collections.Generic;

namespace PIF.EBP.Application.Shared
{
    public class EntityNames
    {
        public const string Contact = "contact";
        public const string Account = "account";
        public const string Otp = "pwc_otp";
        public const string Sms = "hexa_sms";
        public const string PortalInvitation = "hexa_portalinvitation";
        public const string ExternalFormConfiguration = "hexa_externalformconfiguration";
        public const string ProcessTemplate = "hexa_processtemplate";
        public const string ProcessStepTemplate = "hexa_processsteptemplate";
        public const string StepTemplate = "hexa_steptemplate";
        public const string Country = "ntw_country";
        public const string City = "ntw_cities";
        public const string Degree = "ntw_degree";
        public const string Institute = "ntw_institute";
        public const string Major = "ntw_major";
        public const string Position = "ntw_position";
        public const string Experience = "ntw_experience";
        public const string Education = "ntw_education";
        public const string HexaRequests = "hexa_request";
        public const string ExtensionObject = "hexa_extensionobject";
        public const string Request = "hexa_request";
        public const string RequestStep = "hexa_requeststep";
        public const string ProcessDocumentTemplate = "hexa_processdocumenttemplate";
        public const string RequestDocument = "hexa_requestdocument";
        public const string Department = "ntw_department";
        public const string ExternalDepartment = "pwc_externaldepartment";
        public const string PortalPinned = "pwc_portalpinned";
        public const string Notifications = "pwc_portalnotifications";
        public const string ShareFeedback = "feedback";
        public const string Membership = "ntw_membership";
        public const string ContactAssociation = "hexa_contactroleassociation";
        public const string PortalRole = "hexa_portalrole";
        public const string HexaStepTransitionTemplate = "hexa_steptransitiontemplate";
        public const string HexaTransitionTemplate = "hexa_transitiontemplate";
        public const string HexaStepStatusTemplate = "hexa_stepstatustemplate";
        public const string Appointment = "appointment";
        public const string ProcessStatusTemplate = "hexa_processstatustemplate";
        public const string TransitionHistory = "hexa_transitionhistory";
        public const string Portalconfiguration = "hexa_portalconfiguration";
        public const string RequestDocumentComment = "hexa_requestdocumentcomment";
        public const string SystemUser = "systemuser";
        public const string OptionSet = "optionset";
        public const string Team = "team";
        public const string ActivityParty = "activityparty";
        public const string Milestone = "pwc_milestone";
        public const string KeyPerformanceIndicatorKPI = "pwc_keyperformanceindicatorkpi";
        public const string PcCommittee = "ntw_pccommittee";
        public const string KnowledgeItem = "pwc_knowledgeitem";
        public const string KnowledgeItemType = "pwc_masterentityknowledgeitemtype";
        public const string DocumentHistory = "pwc_documenthistory";
        public const string PortalComment = "hexa_portalcomment";
        public const string Investment = "pwc_investment";
        public const string SavedQuery = "savedquery";
        public const string GICSSector = "ntw_gicssector";
        public const string ReadReceipt = "pwc_readreceipt";
        public const string CustomerAddress = "customeraddress";
        public const string FileSharingRequestExtension = "pwc_filesharingrequestextension";
        public const string MembershipType = "ntw_membershiptype";
        public const string KnowledgeContentClassification = "pwc_knowledgecontentclassification";
        public const string RuleBook = "pwc_rulebook";
        public const string Chapters = "pwc_chapter";
        public const string Rules = "pwc_rule";
        public const string Opportunity = "pwc_opportunity";
        public const string Division = "ntw_division";
        public const string Attachment = "attachment";
        public const string Email = "email";
        public class LookupDictionaryKeys
        {
            public const string EntityName = "EntityName";
            public const string Id = "Id";
            public const string Name = "Name";
            public const string NameAr = "NameAr";
            public const string AttributeName = "AttributeName";
            public static Dictionary<string, Dictionary<string, string>> LookupConstants = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "country", new Dictionary<string, string>
                    {
                        { EntityName, Country },
                        { Id, "ntw_countryid" },
                        { Name, "ntw_name" },
                        { NameAr, "ntw_arabicname" }
                    }
                },
                {
                    "city", new Dictionary<string, string>
                    {
                        { EntityName, City },
                        { Id, "ntw_citiesid" },
                        { Name, "ntw_name" },
                        { NameAr, "pwc_namear" }
                    }
                },
                {
                    "position", new Dictionary<string, string>
                    {
                        { EntityName, Position },
                        { Id, "ntw_positionid" },
                        { Name, "ntw_name" },
                        { NameAr, "ntw_namear" }
                    }
                },
                {
                    "institute", new Dictionary<string, string>
                    {
                        { EntityName, Institute },
                        { Id, "ntw_instituteid" },
                        { Name, "ntw_name" },
                        { NameAr, "ntw_namearabic" }
                    }
                },
                {
                    "major", new Dictionary<string, string>
                    {
                        { EntityName, Major },
                        { Id, "ntw_majorid" },
                        { Name, "ntw_name" },
                        { NameAr, "ntw_namearabic" }
                    }
                },
                {
                    "experience", new Dictionary<string, string>
                    {
                        { EntityName, Experience },
                        { Id, "ntw_experienceid" },
                        { Name, "ntw_name" },
                        { NameAr, "ntw_name" }
                    }
                },
                {
                    "degree", new Dictionary<string, string>
                    {
                        { EntityName, Degree },
                        { Id, "ntw_degreeid" },
                        { Name, "ntw_name" },
                        { NameAr, "ntw_namearabic" }
                    }
                },
                {
                    "externaldepartment", new Dictionary<string, string>
                    {
                        { EntityName, ExternalDepartment },
                        { Id, "pwc_externaldepartmentid" },
                        { Name, "pwc_name" },
                        { NameAr, "pwc_nameexternaldepartment" }
                    }
                },
                {
                    "processstatustemplate", new Dictionary<string, string>
                    {
                        { EntityName, ProcessStatusTemplate },
                        { Id, "hexa_processstatustemplateid" },
                        { Name, "hexa_nameen" },
                        { NameAr, "hexa_namear" }
                    }
                },
                {
                    "role", new Dictionary<string, string>
                    {
                        { EntityName, PortalRole },
                        { Id, "hexa_portalroleid" },
                        { Name, "hexa_name" },
                        { NameAr, "pwc_namear" }
                    }
                },
                {
                    "externalstatus", new Dictionary<string, string>
                    {
                        { EntityName, HexaStepStatusTemplate },
                        { Id, "hexa_stepstatustemplateid" },
                        { Name, "hexa_nameen" },
                        { NameAr, "hexa_namear" }
                    }
                },
                {
                    "contentclassification", new Dictionary<string, string>
                    {
                        { EntityName, KnowledgeContentClassification },
                        { Id, "pwc_knowledgecontentclassificationid" },
                        { Name, "pwc_name" },
                        { NameAr, "pwc_namear" }
                    }
                },
                {
                    "chapters", new Dictionary<string, string>
                    {
                        { EntityName, Chapters },
                        { Id, "pwc_chapterid" },
                        { Name, "pwc_title" },
                        { NameAr, "pwc_number" }
                    }
                },

            };

            public static Dictionary<string, Dictionary<string, string>> OptiosetConstants = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    OptionSetKey.Nationality, new Dictionary<string, string>
                    {
                        { EntityName, Contact },
                        { AttributeName, "ntw_nationalityset" }
                    }
                },
                {
                    OptionSetKey.Notification, new Dictionary<string, string>
                    {
                        { EntityName, Contact },
                        { AttributeName, "pwc_notificationspreference" }
                    }
                },
                {
                    OptionSetKey.Language, new Dictionary<string, string>
                    {
                        { EntityName, Contact },
                        { AttributeName, "pwc_portallanguagetypecode" }
                    }
                },
                {
                    OptionSetKey.Feedbacktype, new Dictionary<string, string>
                    {
                        {EntityName, ShareFeedback },
                        {AttributeName, "pwc_portaltypeoffeedback" }
                    }
                },
                {
                    OptionSetKey.Supporttype, new Dictionary<string, string>
                    {
                        {EntityName, ShareFeedback },
                        {AttributeName, "pwc_feedbacktypecode" }  //old field is pwc_portalfeedbacktype
                    }
                },
                {
                    OptionSetKey.Associationtatus, new Dictionary<string, string>
                    {
                        {EntityName, ContactAssociation },
                        {AttributeName, "hexa_associationstatustypecode" }  //old field is pwc_portalfeedbacktype
                    }
                },
                {
                    OptionSetKey.NotificationType, new Dictionary<string, string>
                    {
                        {EntityName, Notifications },
                        {AttributeName, "pwc_typetypecode" }
                    }
                },
                 {
                    OptionSetKey.KnowledgeItemCategory, new Dictionary<string, string>
                    {
                        {EntityName, KnowledgeItem },
                        {AttributeName, "pwc_category" }
                    }
                },
                 {
                    OptionSetKey.KnowledgeItemLabel, new Dictionary<string, string>
                    {
                        {EntityName, KnowledgeItem },
                        {AttributeName, "pwc_label" }
                    }
                },
                 {
                    OptionSetKey.KnowledgeItemArticleType, new Dictionary<string, string>
                    {
                        {EntityName, KnowledgeItem },
                        {AttributeName, "pwc_articletype" }
                    }
                },
                 {
                    OptionSetKey.TransitionHistoryActionType, new Dictionary<string, string>
                    {
                        {EntityName, TransitionHistory },
                        {AttributeName, "hexa_actiontype" }
                    }
                },
                 {
                    OptionSetKey.ContactTitleType, new Dictionary<string, string>
                    {
                        {EntityName, Contact },
                        {AttributeName, "ntw_titleset" }
                    }
                },
                 {
                    OptionSetKey.TypeOfRequirments, new Dictionary<string, string>
                    {
                        {EntityName, Rules },
                        {AttributeName, "pwc_typeofrequirementtypecode" }
                    }
                }
                ,
                 {
                    OptionSetKey.PcStage, new Dictionary<string, string>
                    {
                        {EntityName, Rules },
                        {AttributeName, "pwc_pclifecyclestagetypecode" }
                    }
                }
                ,
                 {
                    OptionSetKey.Theme, new Dictionary<string, string>
                    {
                        {EntityName, Rules },
                        {AttributeName, "pwc_themetypecode" }
                    }
                }
            };
        }
    }
    public static class OptionSetKey
    {
        public const string Nationality = "nationality";
        public const string Notification = "notification";
        public const string Language = "language";
        public const string Feedbacktype = "feedbacktype";
        public const string Supporttype = "supporttype";
        public const string Associationtatus = "associationtatus";
        public const string NotificationType = "notificationtype";
        public const string KnowledgeItemCategory = "knowledgeitemcategory";
        public const string KnowledgeItemLabel = "knowledgeitemlabel";
        public const string KnowledgeItemArticleType = "knowledgeitemarticletype";
        public const string TransitionHistoryActionType = "transitionhistoryactiontype";
        public const string ContactTitleType = "contacttitletype";
        public const string TypeOfRequirments = "pwc_typeofrequirementtypecode";
        public const string PcStage = "pwc_pclifecyclestagetypecode";
        public const string Theme = "pwc_themetypecode";


    }
    public static class OtpTypeNames
    {
        public const string Login = "LoginOtp";
        public const string UpdateProfile = "UpdateProfileOtp";
        public const string ForgetPassword = "ForgetPasswordOtp";
        public const string Invitation = "InvitationOtp";
        public const string Countries = "ntw_country";
        public const string Cities = "ntw_cities";
        public const string Degrees = "ntw_degree";
        public const string Institutes = "ntw_institute";
        public const string Majors = "ntw_major";
        public const string Positions = "ntw_position";
        public const string Experiences = "ntw_experience";
        public const string Education = "ntw_education";
    }
    public static class PageRoute
    {

        public const string Home = "/home";
        public const string AddContact = "/contacts/add";
        public const string Calendar = "/calendar";
        public const string Contact = "/contacts";
        public const string EditContact = "/contacts/edit";
        public const string ForgotPassword = "/forgot-password";
        public const string RestPassword = "/login/reset-password/new-password";
        public const string VerifyPassword = "/forgot-password/verify";
        public const string Login = "/login";
        public const string LoginSelectProfile = "/login/select-profile";
        public const string LoginVerify = "/login/verify";
        public const string RegistrationCreatePassword = "/registration/create";
        public const string RegistrationVerify = "/registration/verify";
        public const string Request = "/request";
        public const string RequestDetails = "/request/details";
        public const string Settings = "/settings";
        public const string TaskDetails = "/request/details/:requestId/:requestStepId";
        public const string RequestNew= "/request/new";
        public const string Files = "/files";

    }
    public static class PermissionType
    {
        public const string Read = "1";
        public const string Create = "2";
        public const string Write = "3";
        public const string Delete = "4";
    }

    public static class Constants
    {
        public const string LangEn = "en";
        public const string LangAr = "ar";
    }
}

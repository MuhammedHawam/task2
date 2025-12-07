using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PIF.EBP.Application.Shared
{
    public class Enums
    {
        public enum YesOrNo
        {
            No = 0,
            Yes = 1
        }
        public enum LogDestination
        {
            Mixed = 0,
            File = 1,
            SQL = 2
        }
        public enum PortalAssociationStatus
        {
            Active = 0,
            Inactive = 1,
            Invited = 2,
            Registered = 3,
            Expired = 4,
            Locked = 5,
            PendingApproval = 6,
            Deleted = 7
        }

        public enum AccessLevel
        {
            /// <summary>
            /// Grants access to records in the user's business unit and all subordinate business units.
            /// This level includes all permissions granted by the Local and Basic levels.
            /// Commonly referred to as "Parent: Child Business Units" access.
            /// </summary>
            Deep = 0,
            /// <summary>
            ///This access level gives a user access to records that the user owns,
            ///objects that are shared with the user, and objects that are shared with a team that the user is a member of.
            ///The application refers to this access level as User.
            /// </summary>
            Basic = 1,
            /// <summary>
            ///No access allowed
            /// </summary>
            None = 2,
            /// <summary>
            ///User level permission, where he can access stuff assigned to him only.
            /// </summary>
            User = 3
        }
        public enum RoleType
        {
            BoardMember = 117280000,
            NonBoardMember = 117280001
        }

        public enum OtpType
        {
            Login = 117280000,
            UpdateProfile = 117280001,
            ForgetPassword = 117280002,
            Invitation = 117280003
        }

        public enum RequestDocStatusType
        {
            Active = 1,
            ReUpload = 110000000,
            PendingUpload = 110000001,
            Uploaded = 110000002,
            Rejected = 110000005,
            SharedFromPreviousStep = 954660000
        }

        public enum Roles
        {
            DecisionMaker = 1,
            Employee = 2,
            Influencer = 3
        }

        public enum Nationalities
        {
            Saudi = 961110000,
            Non_Saudi = 961110001
        }

        public enum ScopeWidgetData
        {
            All = 0,
            RequestSummary = 1,
            Tasks = 2,
            Schedule = 3
        }

        public enum ScopeTask
        {
            All = 0,
            Pending = 1,
            Completed = 2,
        }

        public enum ScopeSchedule
        {
            All = 0,
            Meetings = 1,
            Events = 2,
            DueDates = 3,
        }
        public enum ScopeSearchCalender
        {
            Meetings = 0,
            Events = 1,
            Tasks = 2,
            Requests = 3,
        }
        public enum PortalInvitationStatus
        {
            Invited = 0,
            Registered = 1,
            Inactive = 2,
            Expired = 3,
        }

        public enum PortalUserStatus
        {
            Active = 0,
            Inactive = 1,
            Locked = 2,
            NotApplicable = 3,
        }

        public enum PortalLanguage
        {
            English = 117280000,
            Arabic = 117280001
        }
        public enum SortOrder
        {
            Ascending = 0,
            Descending = 1
        }
        public enum MatchMode
        {
            dateIs = 25,
            dateIsNot = 11,
            dateBefore = 26,
            dateAfter = 27,
            startsWith = 54,
            contains = 6,
            notContains = 7,
            endsWith = 56,
            equals = 0,
            notEquals = 1
        }
        public enum Operator
        {
            and = 1,
            or = 2
        }
        public enum ServicesFilter
        {
            All = 1,
            Pending = 2,
            Completed = 2
        }
        public enum RSVP
        {
            Pending = 1,
            Accept = 2,
            Decline = 3
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
        public static List<string> GetEnumDescriptions<T>(int[] values)
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type must be an enum");
            }

            return type.GetFields(BindingFlags.Public | BindingFlags.Static)
                       .Where(field => values.Contains((int)field.GetValue(null)))
                       .Select(field => field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name)
                       .ToList();
        }

        public enum ExternalFormType
        {
            ProcessTemplate = 0,
            ProcessStepTemplate = 1
        }

        public enum HexaInternalStatusType
        {
            start = 110000000,
            Intermediate = 110000001,
            End = 110000002
        }

        public enum NotificationType
        {
            Appointment = 117280000,
            Task = 117280001
        }

        public enum PortalNotificationType
        {
            NewInvitation = 0,
            UpdatedInvitation = 1,
            NewTaskReceived = 2,
            TaskAssigned = 3,
            RequestOverdue = 4,
            ReturnedTask = 5,
            ReturnedRequest = 6,
            FileDeletion = 7
        }

        public enum NotificationReadStatus
        {
            Read = 117280000,
            Unread = 117280001
        }

        public enum ShareFeedbackType
        {
            ShareFeedback = 117280003,
            ReportBug = 117280000
        }

        public enum FeedbackType
        {
            Complaint = 2,
            suggestion = 0
        }

        public enum AppointmentType
        {
            Event = 117280001,
            Meeting = 117280000
        }

        public enum StateCode
        {
            Active = 0,
            Inactive = 1,
        }

        public enum ExternalUser
        {
            Yes = 0,
            No = 1,
        }

        public enum ExecutiveManagement
        {
            Yes = 961110000,
            no = 961110001
        }

        public enum CompanyGovernanceManagementScope
        {
            All = 0,
            BorderMembers = 1,
            EBPUsers = 2,
            CommitteeMembers = 3,
        }

        public enum CompanyKPIsMilestonesScope
        {
            All = 0,
            KPIs = 1,
            Milestones = 2
        }

        public enum FileOverriddenAction
        {
            NoAction = 0,
            KeepBoth = 1,
            Replace = 2
        }

        public enum AnnouncementCategory
        {
            All = 0,
            Important = 517430000,
            Sensitive = 517430003,
            Public = 517430001,
        }

        public enum ArticleCategory
        {
            All = -1,
            Posts = 0,
            Report = 1,
            Whitepapers = 2,
            Newsletters = 3
        }

        public enum KnowledgeItemType
        {
            All = 0,
            Article = 1,
            Announcement = 2,
            FAQ = 3,
            Template = 4,
            Playbook=5,
            UserManual=6
        }

        public enum PinnedKnowledgeItemType
        {
            All = 0,
            Article = 517430001,
            Announcement = 517430002,
            FAQ = 517430003,
            Playbook = 517430004,
            Template = 517430005,
            Mannual = 517430000
        }
        public enum RequestStepOwnership
        {
            External=1,
            Internal=2,
            Both=3
        }
        public enum MembershipType
        {
            PCBoardChairman = 1,
            PCBoardMember = 2,
            PCBoardViceChairman = 3,
            PCCommitteeChairman = 4,
            PCCommitteeMember = 5,
            PCCommitteeViceChairman = 6,
            ProjectBoardChairman = 7,
            ProjectBoardMember = 8,
            ProjectBoardViceChairman = 9,
            ProjectCommitteeChairman = 10,
            ProjectCommitteeMember = 11,
            ProjectCommitteeViceChairman = 12
        }
        public enum RuleBookType
        {
            All = 1,
            PC = 2,
            PIF = 3,
        }
        public enum RuleBookStatus
        {
            Draft = 1,
            UnderReview = 2,
            PendingApproval = 3,
            Approved = 4,
            Published =5,
            Deprecated=6,
            Archived=7, 
        }

        /// <summary>
        /// Sorting options for general company lists
        /// </summary>
        public enum CompanySortOrder
        {
            MostActive = 0,      // Default: highest total of challenges + campaigns
            AlphabeticalAZ = 1,  // A–Z by company name
            Newest = 2,          // Most recently created
            Location = 3         // Sorted by location/city
        }

    }
}

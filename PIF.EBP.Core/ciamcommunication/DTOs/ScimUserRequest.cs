using Newtonsoft.Json;
using System.Collections.Generic;

namespace PIF.EBP.Core.CIAMCommunication.DTOs
{
    public class ScimUserRequest
    {
        [JsonProperty("schemas")]
        public IList<string> Schemas { get; set; } = new List<string>
        {
            "urn:ietf:params:scim:schemas:core:2.0:User",
            "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User",
            "urn:scim:wso2:schema",
            "urn:scim:schemas:extension:custom:User"
        };

        [JsonProperty("name")]
        public ScimName Name { get; set; } = new ScimName();

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("emails")]
        public IList<ScimEmail> Emails { get; set; } = new List<ScimEmail>();

        [JsonProperty("phoneNumbers")]
        public IList<ScimPhoneNumber> PhoneNumbers { get; set; } = new List<ScimPhoneNumber>();

        [JsonProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User")]
        public ScimEnterpriseExtension Enterprise { get; set; } = new ScimEnterpriseExtension();

        [JsonProperty("urn:scim:wso2:schema")]
        public ScimWso2Extension Wso2 { get; set; } = new ScimWso2Extension();

        [JsonProperty("urn:scim:schemas:extension:custom:User")]
        public ScimCustomExtension Custom { get; set; } = new ScimCustomExtension();
    }

    public class ScimName
    {
        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("familyName")]
        public string FamilyName { get; set; }
    }

    public class ScimEmail
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; } = true;
    }

    public class ScimPhoneNumber
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "mobile";
    }

    public class ScimWso2Extension
    {
        [JsonProperty("askPassword")]
        public string AskPassword { get; set; } = "false";

        [JsonProperty("country")]
        public string Country { get; set; } = "Saudi Arabia";

        [JsonProperty("accountLocked")]
        public bool AccountLocked { get; set; } = false;

        [JsonProperty("accountState")]
        public string AccountState { get; set; } = "UNLOCKED";
    }

    public class ScimCustomExtension
    {
        [JsonProperty("companyId")]
        public string CompanyId { get; set; }

        [JsonProperty("ContactID")]
        public string ContactID { get; set; }

        [JsonProperty("RoleID")]
        public IList<string> RoleID { get; set; } = new List<string>();

        [JsonProperty("participant")]
        public string Participant { get; set; }
    }

    public class ScimUserResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("schemas")]
        public IList<string> Schemas { get; set; }

        [JsonProperty("meta")]
        public ScimMeta Meta { get; set; }
    }

    public class ScimMeta
    {
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("lastModified")]
        public string LastModified { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }

    public class ScimOperationResponse
    {
        public bool IsSuccess { get; set; }
        public string Id { get; set; }
        public string RawResponse { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ScimPatchRequest
    {
        [JsonProperty("schemas")]
        public IList<string> Schemas { get; set; } = new List<string>
        {
            "urn:ietf:params:scim:api:messages:2.0:PatchOp"
        };

        [JsonProperty("Operations")]
        public IList<ScimPatchOperation> Operations { get; set; } = new List<ScimPatchOperation>();
    }

    public class ScimPatchOperation
    {
        [JsonProperty("op")]
        public string Op { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }
    }

    public class ScimEnterpriseExtension
    {
        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("accountLocked")]
        public bool? AccountLocked { get; set; }

        [JsonProperty("accountDisabled")]
        public bool? AccountDisabled { get; set; }
    }

    public class ScimEnterpriseReplaceRequest
    {
        [JsonProperty("schemas")]
        public IList<string> Schemas { get; set; } = new List<string>
        {
            "urn:ietf:params:scim:schemas:core:2.0:User",
            "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User",
            "urn:scim:wso2:schema"
        };

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User")]
        public ScimEnterpriseExtension Enterprise { get; set; } = new ScimEnterpriseExtension();
    }

    public class ScimUserDetailResponse : ScimUserResponse
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("emails")]
        public IList<string> Emails { get; set; }

        [JsonProperty("name")]
        public ScimName Name { get; set; }

        [JsonProperty("phoneNumbers")]
        public IList<ScimPhoneNumber> PhoneNumbers { get; set; }

        [JsonProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User")]
        public ScimEnterpriseExtension Enterprise { get; set; }

        [JsonProperty("urn:scim:wso2:schema")]
        public ScimWso2Extension Wso2 { get; set; }

        [JsonProperty("urn:scim:schemas:extension:custom:User")]
        public ScimCustomExtension Custom { get; set; }

        [JsonProperty("ims")]
        public IList<dynamic> Ims { get; set; }

        [JsonProperty("roles")]
        public IList<dynamic> Roles { get; set; }
    }

    public class ScimUserListResponse
    {
        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }

        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        [JsonProperty("itemsPerPage")]
        public int ItemsPerPage { get; set; }

        [JsonProperty("schemas")]
        public IList<string> Schemas { get; set; }

        [JsonProperty("Resources")]
        public IList<ScimUserDetailResponse> Resources { get; set; }
    }

    public class ScimListOperationResponse
    {
        public bool IsSuccess { get; set; }
        public ScimUserListResponse Payload { get; set; }
        public string RawResponse { get; set; }
        public string ErrorMessage { get; set; }
    }
}

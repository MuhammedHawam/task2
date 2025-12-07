using PIF.EBP.Core.FileManagement.DTOs;
using System.Collections.Generic;

namespace PIF.EBP.Core.Utilities
{
    public class DocumentMetadata
    {
        public const string FieldName = "FieldName";
        public const string FieldType = "FieldType";
        public const string DisplayName = "DisplayName";
        public static Dictionary<string, Dictionary<string, string>> MetadataConstants = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "CompanyId", new Dictionary<string, string>
                    {
                        { FieldName, "CompanyId" },
                        { FieldType, "Text" },
                        { DisplayName, "CompanyId" }
                    }
                },
                {
                    "ModifiedBy", new Dictionary<string, string>
                    {
                        { FieldName, "modified_by_contact_id" },
                        { FieldType, "Text" },
                        { DisplayName, "modified_by_contact_id" }
                    }
                },
                {
                    "CreatedBy", new Dictionary<string, string>
                    {
                        { FieldName, "created_by_contact_Id" },
                        { FieldType, "Text" },
                        { DisplayName, "created_by_contact_Id" }
                    }
                },
                {
                    "ModifiedOn", new Dictionary<string, string>
                    {
                        { FieldName, "modified_by_contact_DateTime" },
                        { FieldType, "DateTime" },
                        { DisplayName, "modified_by_contact_DateTime" }
                    }
                },
            {
                    "SubFolderId", new Dictionary<string, string>
                    {
                        { FieldName, "SubFolderId" },
                        { FieldType, "Text" },
                        { DisplayName, "SubFolderId" }
                    }
                },
             {
                    "FolderId", new Dictionary<string, string>
                    {
                        { FieldName, "FolderId" },
                        { FieldType, "Text" },
                        { DisplayName, "FolderId" }
                    }
                },{
                    "ContactId", new Dictionary<string, string>
                    {
                        { FieldName, "ContactId" },
                        { FieldType, "Text" },
                        { DisplayName, "ContactId" }
                    }
                }
        };
        public static DocumentMetadataDto GetDocumentMetadataByKey(string key)
        {
            if (DocumentMetadata.MetadataConstants.TryGetValue(key, out var metadata))
            {
                return new DocumentMetadataDto
                {
                    DisplayName = metadata[DocumentMetadata.DisplayName],
                    FieldName = metadata[DocumentMetadata.FieldName],
                    FieldType = metadata[DocumentMetadata.FieldType],

                };
            }

            return null;
        }
        public static string GetDocMetadataFieldName(string key)
        {
            if (DocumentMetadata.MetadataConstants.TryGetValue(key, out var metadata))
            {
                return metadata[DocumentMetadata.FieldName];

            }
            return string.Empty;
        }
    }
}


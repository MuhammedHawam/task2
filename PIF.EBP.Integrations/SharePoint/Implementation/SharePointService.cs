using DocumentFormat.OpenXml.Packaging;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.SharePoint.Client;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement;
using PIF.EBP.Core.FileManagement.Consts;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.Core.Helpers;
using PIF.EBP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using static PIF.EBP.Core.Shared.Enums;
using Folder = Microsoft.SharePoint.Client.Folder;

namespace PIF.EBP.Integrations.SharePoint.Implementation
{
    public class SharePointService : IFileManagement
    {
        public SharePointService()
        {

        }

        public async Task<DocumentInfo> RetrieveDocumentAsync(RetrieveDocumentDto oRetrieveDocumenDto)
        {
            GetFileFromSPByPath(oRetrieveDocumenDto.AttachmentPath, out string fileName, out string base64content, oRetrieveDocumenDto.User);

            return new DocumentInfo()
            {
                DocumentName = fileName,
                DocumentContent = base64content
            };
        }
        private ClientContext GetSpConnection()
        {
            var SPSiteCollectionUri = ConfigurationManager.AppSettings["SPSiteUri"];
            var encSPUsername = ConfigurationManager.AppSettings["SPUsername"];
            var SPDomainname = ConfigurationManager.AppSettings["SPDomainname"];
            var encPassword = ConfigurationManager.AppSettings["SPPass"];


            var SPUsername = CryptoUtils.Decrypt(encSPUsername);
            var Password = CryptoUtils.Decrypt(encPassword);

            ClientContext SpContext = new ClientContext(SPSiteCollectionUri);
            var SharePointNwCredential = new NetworkCredential(SPUsername, Password, SPDomainname);
            SpContext.Credentials = SharePointNwCredential;
            SpContext.RequestTimeout = System.Threading.Timeout.Infinite;
            return SpContext;
        }
        private ClientContext GetSpConnection_Ext()
        {
            var SPSiteCollectionUri = ConfigurationManager.AppSettings["SPSiteUri_ext"];
            var encSPUsername = ConfigurationManager.AppSettings["SPUsername_ext"];
            var SPDomainname = ConfigurationManager.AppSettings["SPDomainname_ext"];
            var encPassword = ConfigurationManager.AppSettings["SPPass_ext"];

            var SPUsername = CryptoUtils.Decrypt(encSPUsername);
            var Password = CryptoUtils.Decrypt(encPassword);

            ClientContext SpContext = new ClientContext(SPSiteCollectionUri);
            var SharePointNwCredential = new NetworkCredential(SPUsername, Password, SPDomainname);
            SpContext.Credentials = SharePointNwCredential;
            SpContext.RequestTimeout = System.Threading.Timeout.Infinite;
            return SpContext;
        }


        private ClientContext GetSpConnection_Private()
        {
            var SPSiteCollectionUri = ConfigurationManager.AppSettings["SPSiteUri_private"];
            var encSPUsername = ConfigurationManager.AppSettings["SPUsername_private"];
            var SPDomainname = ConfigurationManager.AppSettings["SPDomainname_private"];
            var encPassword = ConfigurationManager.AppSettings["SPPass_private"];

            var SPUsername = CryptoUtils.Decrypt(encSPUsername);
            var Password = CryptoUtils.Decrypt(encPassword);

            ClientContext SpContext = new ClientContext(SPSiteCollectionUri);
            var SharePointNwCredential = new NetworkCredential(SPUsername, Password, SPDomainname);
            SpContext.Credentials = SharePointNwCredential;
            SpContext.RequestTimeout = System.Threading.Timeout.Infinite;
            return SpContext;
        }

        public int CopyDocumentFiles(CopyFiles document, string source, string skip, string user,
            string sourceLogical, string destinationObjectName, out int itemCount, ref string relativeUrlFolderPath)
        {
            int successCount = 0;
            using (ClientContext spContext = GetSpConnection())
            {
                User thisUser = ImpersonateUser(user, spContext);

                var sourceFolder =
                    spContext.Web.GetFolderByServerRelativeUrl(string.Join("/", spContext.Url.Trim('/'), sourceLogical,
                        source));

                FileCollection fileslist = sourceFolder.Files;
                spContext.Load(fileslist, inc => inc.Include(
                    i => i.Exists,
                    i => i.ServerRelativeUrl,
                    i => i.Name
                ));

                spContext.ExecuteQuery();

                itemCount = fileslist.Count;
                if (fileslist.Any())
                {
                    Uri currUrl = new Uri(spContext.Url);
                    string destination = destinationObjectName;
                    relativeUrlFolderPath = string.Join("/", spContext.Url.Trim('/'), SharePointDocumentLocation.RegardingObjectName, destination);

                    var targetFolder = spContext.Web.GetFolderByServerRelativeUrl(relativeUrlFolderPath);

                    ImpersonateFolder(thisUser, sourceFolder, targetFolder);

                    fileslist.Where(item =>
                    (item != null && item.Exists) && document.MoveAll == false ?
                    (document.ListFilesName == null ? true :
                    document.ListFilesName.Any(fileName => fileName.ToLower().Equals(item.Name.ToLower()))) :
                    true)
                   .ToList().ForEach(file =>
                   {
                       try
                       {
                           if ((string.IsNullOrWhiteSpace(skip) ? true : !file.ServerRelativeUrl.Contains(skip)))
                           {
                               ImpersonateFile(thisUser, file);

                               file.RefreshLoad();
                               spContext.Load(file);

                               string destFileUrl = string.Join("/", new Uri(spContext.Url).AbsolutePath,
                                   SharePointDocumentLocation.RegardingObjectName, destination, file.Name);

                               file.CopyTo(destFileUrl, true);
                               spContext.ExecuteQuery();

                               var targetfile = spContext.Web.GetFileByServerRelativeUrl(destFileUrl);
                               targetfile.RefreshLoad();
                               ImpersonateFile(thisUser, targetfile);
                               spContext.Load(targetfile);

                               successCount++;
                           }
                       }
                       catch { }
                   });
                    spContext.ExecuteQuery();
                }
            }

            return successCount;
        }

        public List<GetDocumentListRsp> SPListItems(string targetFileURL)
        {
            List<GetDocumentListRsp> listFiles = new List<GetDocumentListRsp>();
            using (ClientContext spContext = GetSpConnection())
            {
                var fileslist = spContext.Web.GetFolderByServerRelativeUrl(targetFileURL).Files;
                spContext.Load(fileslist, inc => inc.Include(
                    i => i.Exists,
                    i => i.ListId,
                    i => i.UniqueId,
                    i => i.Name,
                    i => i.ServerRelativeUrl,
                    i => i.TimeCreated,
                    i => i.Length
                ));

                spContext.ExecuteQuery();
                fileslist.Where(item => item != null && item.Exists).ToList().ForEach(file =>
                {
                    Uri currUrl = new Uri(spContext.Url);
                    string FullUrl = currUrl.AbsoluteUri.ToLower().Replace(currUrl.AbsolutePath, string.Empty) +
                                     file.ServerRelativeUrl;

                    listFiles.Add(new GetDocumentListRsp
                    {
                        DocumentName = file.Name,
                        DocumentCreatedOnInUTC = file.TimeCreated,
                        DocumentSizeInBytes = file.Length,
                        DocumentType = System.IO.Path.GetExtension(file.Name).ToLower()
                        
                    });

                });
                return listFiles;
            }
        }
        public List<GetDocumentListRsp> GetDocumentsByTargetUrl(string targetURL)
        {
            List<GetDocumentListRsp> listFiles = new List<GetDocumentListRsp>();
            try
            {
                using (ClientContext spContext = GetSpConnection_Ext())
                {
                    var fileslist = spContext.Web.GetFolderByServerRelativeUrl(targetURL).Files;
                    spContext.Load(fileslist, inc => inc.Include(
                        i => i.Exists,
                        i => i.ListId,
                        i => i.UniqueId,
                        i => i.Name,
                        i => i.ServerRelativeUrl,
                        i => i.TimeCreated,
                        i => i.Length
                    ));

                    spContext.ExecuteQuery();
                    fileslist.Where(item => item != null && item.Exists).ToList().ForEach(file =>
                    {
                        Uri currUrl = new Uri(spContext.Url);
                        string FullUrl = currUrl.AbsoluteUri.ToLower().Replace(currUrl.AbsolutePath, string.Empty) +
                                         file.ServerRelativeUrl;
                        listFiles.Add(new GetDocumentListRsp
                        {
                            DocumentName = file.Name,
                            DocumentCreatedOnInUTC = file.TimeCreated,
                            DocumentSizeInBytes = file.Length,
                            DocumentType = System.IO.Path.GetExtension(file.Name).ToLower()
                        });

                    });
                    return listFiles;
                }
            }
            catch (Exception)
            {

                return listFiles;
            }
        }

        public GetDocumentListRspBase GetDocuments(GetDocumentListDto searchDocumentListDto)
        {
            GetDocumentListRspBase getDocumentListRspBase = new GetDocumentListRspBase();

            using (ClientContext spContext = GetSpConnection_Ext())
            {
                getDocumentListRspBase = GetFilesAndFolders(spContext, searchDocumentListDto);
            }

            return getDocumentListRspBase;
        }

        private GetDocumentListRspBase GetFilesAndFolders(ClientContext spContext, GetDocumentListDto searchDocumentListDto)
        {
            Dictionary<string, object> fileMetadata = null;
            GetDocumentListRspBase getDocumentListRspBase = new GetDocumentListRspBase();
            List<GetDocumentListRsp> listFiles = new List<GetDocumentListRsp>();

            string[] targetFoldersURL = searchDocumentListDto.targetFolderURL.Split(',');

            foreach (string folderURL in targetFoldersURL)
            {
                int fileId = 0;

                spContext.Load(spContext.Web);
                spContext.ExecuteQuery();
                string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                string sortOrder = searchDocumentListDto.PagingRequest.SortOrder == SortOrder.Ascending ? "TRUE" : "FALSE";
                string sortField = GetCamlFieldName(searchDocumentListDto.PagingRequest.SortField);

                CamlQuery paginatedQuery = new CamlQuery
                {
                    ViewXml = $@"
                    <View Scope='RecursiveAll'>
                        <Query>
                            <OrderBy>
                                <FieldRef Name='{sortField}' Ascending='{sortOrder}' />
                            </OrderBy>
                            <Where>
                                <And>
                                    <IsNotNull>
                                        <FieldRef Name='FileRef' />
                                    </IsNotNull>
                                    <Gt>
                                        <FieldRef Name='File_x0020_Size' />
                                        <Value Type='Number'>0</Value>
                                    </Gt>
                                </And>
                            </Where>
                        </Query>
                    </View>",
                    FolderServerRelativeUrl = webRelativeUrl + folderURL
                };

                ListItemCollection files = spContext.Web.GetList(webRelativeUrl + folderURL).GetItems(paginatedQuery);
                spContext.Load(files, items => items.Include(
                    i => i["FileLeafRef"],
                    i => i["FileRef"],
                    i => i["Created"],
                    i => i["File_x0020_Size"],
                    i => i["Author"],
                    i => i["Editor"],
                    i => i["Modified"]
                ));
                spContext.ExecuteQuery();

                foreach (var item in files)
                {
                    string fileUrl = item["FileRef"].ToString();
                    string fileName = item["FileLeafRef"].ToString();
                    DateTime fileCreated = (DateTime)item["Created"];
                    long fileSize = (string)item["File_x0020_Size"] != string.Empty ? long.Parse((string)item["File_x0020_Size"]) : 0;
                    string createdBy = ((FieldUserValue)item["Author"]).LookupValue;
                    string modifiedBy = ((FieldUserValue)item["Editor"]).LookupValue;
                    DateTime modifiedDate = (DateTime)item["Modified"];

                    if (fileSize > 0)
                    {
                        fileId++;

                        FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(spContext, fileUrl);
                        using (MemoryStream fileStream = new MemoryStream())
                        {
                            string fileExtension = System.IO.Path.GetExtension(fileName).ToLower();

                            FileMetadata fileMetadataobj = GetFileMetadata(spContext, item.File, modifiedDate);
                            GetDocumentListRsp foldersAttributeobj = GetFileFoldersAttribute(fileUrl);

                            listFiles.Add(new GetDocumentListRsp
                            {
                                DocumentName = fileName,
                                DocumentId = fileId.ToString(),
                                DocumentCreatedOnInUTC = fileCreated,
                                DocumentSizeInBytes = fileSize,
                                DocumentPath = fileUrl.Substring(webRelativeUrl.Length),
                                DocumentType = !string.IsNullOrEmpty(fileExtension) ? fileExtension.TrimStart('.').ToUpper() : string.Empty,
                                DocumentMetadata = fileMetadata,
                                DocumentCreatedBy = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.CreatedBy) ? fileMetadataobj.CreatedBy : createdBy,
                                DocumentCreatedByAr = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.CreatedByAr) ? fileMetadataobj.CreatedByAr : createdBy,
                                DocumentModifiedBy = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.ModifiedBy) ? fileMetadataobj.ModifiedBy : modifiedBy,
                                DocumentModifiedByAr = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.ModifiedByAr) ? fileMetadataobj.ModifiedByAr : modifiedBy,
                                DocumentModifiedOnInUTC = modifiedDate,
                                IsExternalModifiedByUser = fileMetadataobj != null ? fileMetadataobj.ExternalModifiedBy : true,
                                FolderId = foldersAttributeobj.FolderId,
                                FolderName = foldersAttributeobj.FolderName,
                                SubFolderId = foldersAttributeobj.SubFolderId,
                                SubFolderName = foldersAttributeobj.SubFolderName
                            });
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(searchDocumentListDto.PagingRequest.SortField) && searchDocumentListDto.PagingRequest.SortField == "documentType")
            {
                if (searchDocumentListDto.PagingRequest.SortOrder == SortOrder.Ascending)
                {
                    listFiles = listFiles.OrderBy(x => x.DocumentType).ToList();
                }
                else
                {
                    listFiles = listFiles.OrderByDescending(x => x.DocumentType).ToList();
                }
            }

            listFiles.RemoveAll(x => string.IsNullOrEmpty(x.SubFolderId));

            getDocumentListRspBase.ItemCount = listFiles.Count;
            getDocumentListRspBase.GetDocumentListRsp = listFiles;

            if (searchDocumentListDto.PagingRequest != null && searchDocumentListDto.PagingRequest.PageNo > 0)
            {
                int pageNo = searchDocumentListDto.PagingRequest.PageNo;
                int pageSize = searchDocumentListDto.PagingRequest.PageSize;

                getDocumentListRspBase.GetDocumentListRsp = listFiles.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
            }

            return getDocumentListRspBase;
        }

        public GetDocumentListRspBase SearchDocuments(SearchDocumentListDto searchDocumentListDto)
        {
            GetDocumentListRspBase getDocumentListRspBase = new GetDocumentListRspBase();

            using (ClientContext spContext = GetSpConnection_Ext())
            {
                getDocumentListRspBase = SearchFilesAndFolders(spContext, searchDocumentListDto);
            }

            return getDocumentListRspBase;
        }

        private GetDocumentListRspBase SearchFilesAndFolders(ClientContext spContext, SearchDocumentListDto searchDocumentListDto)
        {
            Dictionary<string, object> fileMetadata = null;
            GetDocumentListRspBase getDocumentListRspBase = new GetDocumentListRspBase();
            List<GetDocumentListRsp> listFiles = new List<GetDocumentListRsp>();

            string[] targetFoldersURL = searchDocumentListDto.targetFolderURL.Split(',');

            foreach (string folderURL in targetFoldersURL)
            {
                int fileId = 0;

                spContext.Load(spContext.Web);
                spContext.ExecuteQuery();
                string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                string sortOrder = searchDocumentListDto.PagingRequest.SortOrder == SortOrder.Ascending ? "TRUE" : "FALSE";
                string sortField = GetCamlFieldName(searchDocumentListDto.PagingRequest.SortField);

                CamlQuery paginatedQuery = new CamlQuery
                {
                    ViewXml = $@"
                    <View Scope='RecursiveAll'>
                        <Query>
                            <OrderBy>
                                <FieldRef Name='{sortField}' Ascending='{sortOrder}' />
                            </OrderBy>
                            <Where>
                                <And>
                                    <IsNotNull>
                                        <FieldRef Name='FileRef' />
                                    </IsNotNull>
                                    <Gt>
                                        <FieldRef Name='File_x0020_Size' />
                                        <Value Type='Number'>0</Value>
                                    </Gt>
                                </And>
                            </Where>
                        </Query>
                    </View>",
                    FolderServerRelativeUrl = webRelativeUrl + folderURL
                };

                ListItemCollection files = spContext.Web.GetList(webRelativeUrl + folderURL).GetItems(paginatedQuery);
                spContext.Load(files, items => items.Include(
                    i => i["FileLeafRef"],
                    i => i["FileRef"],
                    i => i["Created"],
                    i => i["File_x0020_Size"],
                    i => i["Author"],
                    i => i["Editor"],
                    i => i["Modified"]
                ));
                spContext.ExecuteQuery();

                foreach (var item in files)
                {
                    string fileUrl = item["FileRef"].ToString();
                    string fileName = item["FileLeafRef"].ToString();
                    DateTime fileCreated = (DateTime)item["Created"];
                    long fileSize = (string)item["File_x0020_Size"] != string.Empty ? long.Parse((string)item["File_x0020_Size"]) : 0;
                    string createdBy = ((FieldUserValue)item["Author"]).LookupValue;
                    string modifiedBy = ((FieldUserValue)item["Editor"]).LookupValue;
                    DateTime modifiedDate = (DateTime)item["Modified"];

                    if (fileSize > 0)
                    {
                        fileId++;

                        FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(spContext, fileUrl);
                        using (MemoryStream fileStream = new MemoryStream())
                        {
                            fileInfo.Stream.CopyTo(fileStream);
                            fileStream.Seek(0, SeekOrigin.Begin);

                            bool found = false;
                            string fileExtension = System.IO.Path.GetExtension(fileName).ToLower();

                            found = SearchByFileName(fileName, searchDocumentListDto.searchText);

                            if (!found)
                            {
                                found = SearchByFileMetadata(spContext, item.File, searchDocumentListDto.searchText, out fileMetadata);
                            }

                            if (!found)
                            {
                                switch (fileExtension)
                                {
                                    case ".txt":
                                        found = SearchInTextFile(fileStream, searchDocumentListDto.searchText);
                                        break;
                                    case ".docx":
                                        found = SearchInWordDocument(fileStream, searchDocumentListDto.searchText);
                                        break;
                                    case ".xlsx":
                                        found = SearchInExcelFile(fileStream, searchDocumentListDto.searchText);
                                        break;
                                    case ".pptx":
                                        found = SearchInPowerPoint(fileStream, searchDocumentListDto.searchText);
                                        break;
                                    case ".pdf":
                                        found = SearchInPdfFile(fileStream, searchDocumentListDto.searchText);
                                        break;
                                }
                            }

                            if (found)
                            {
                                FileMetadata fileMetadataobj = GetFileMetadata(spContext, item.File, modifiedDate);
                                GetDocumentListRsp foldersAttributeobj = GetFileFoldersAttribute(fileUrl);

                                listFiles.Add(new GetDocumentListRsp
                                {
                                    DocumentName = fileName,
                                    DocumentId = fileId.ToString(),
                                    DocumentCreatedOnInUTC = fileCreated,
                                    DocumentSizeInBytes = fileSize,
                                    DocumentPath = fileUrl.Substring(webRelativeUrl.Length),
                                    DocumentType = !string.IsNullOrEmpty(fileExtension) ? fileExtension.TrimStart('.').ToUpper() : string.Empty,
                                    DocumentMetadata = fileMetadata,
                                    DocumentCreatedBy = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.CreatedBy) ? fileMetadataobj.CreatedBy : createdBy,
                                    DocumentCreatedByAr = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.CreatedByAr) ? fileMetadataobj.CreatedByAr : createdBy,
                                    DocumentModifiedBy = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.ModifiedBy) ? fileMetadataobj.ModifiedBy : modifiedBy,
                                    DocumentModifiedByAr = fileMetadataobj != null && !string.IsNullOrEmpty(fileMetadataobj.ModifiedByAr) ? fileMetadataobj.ModifiedByAr : modifiedBy,
                                    DocumentModifiedOnInUTC = modifiedDate,
                                    IsExternalModifiedByUser = fileMetadataobj != null ? fileMetadataobj.ExternalModifiedBy : true,
                                    FolderId = foldersAttributeobj.FolderId,
                                    FolderName = foldersAttributeobj.FolderName,
                                    SubFolderId = foldersAttributeobj.SubFolderId,
                                    SubFolderName = foldersAttributeobj.SubFolderName
                                });
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(searchDocumentListDto.PagingRequest.SortField) && searchDocumentListDto.PagingRequest.SortField == "documentType")
            {
                if (searchDocumentListDto.PagingRequest.SortOrder == SortOrder.Ascending)
                {
                    listFiles = listFiles.OrderBy(x => x.DocumentType).ToList();
                }
                else
                {
                    listFiles = listFiles.OrderByDescending(x => x.DocumentType).ToList();
                }
            }

            listFiles.RemoveAll(x => string.IsNullOrEmpty(x.SubFolderId));

            getDocumentListRspBase.ItemCount = listFiles.Count;
            getDocumentListRspBase.GetDocumentListRsp = listFiles;

            if (searchDocumentListDto.PagingRequest != null && searchDocumentListDto.PagingRequest.PageNo > 0)
            {
                int pageNo = searchDocumentListDto.PagingRequest.PageNo;
                int pageSize = searchDocumentListDto.PagingRequest.PageSize;

                getDocumentListRspBase.GetDocumentListRsp = listFiles.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
            }

            return getDocumentListRspBase;
        }

        private GetDocumentListRsp GetFileFoldersAttribute(string fileUrl)
        {
            GetDocumentListRsp getDocumentListRsp = new GetDocumentListRsp();

            var segments = fileUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 5)
            {
                throw new ArgumentException("File URL does not have enough segments.");
            }
            var siteUrl = "/" + segments[0] + "/" + segments[1];
            var documentLibrary = "/" + segments[2];
            var entityName = "/" + segments[3];
            var rootFolderId = "/" + segments[4];

            var folderId = GetFolderIdByUrl(siteUrl + documentLibrary + entityName + rootFolderId, GetMetadataFieldName("FolderId"));
            var subFolderId = GetFolderIdByUrl(siteUrl + documentLibrary + entityName + rootFolderId, GetMetadataFieldName("SubFolderId"));

            if (folderId != null && subFolderId != null)
            {
                getDocumentListRsp.FolderName = segments[3];
                getDocumentListRsp.FolderId = folderId;
                getDocumentListRsp.SubFolderName = segments[4];
                getDocumentListRsp.SubFolderId = subFolderId;
            }

            return getDocumentListRsp;
        }

        private string GetFolderIdByUrl(string folderUrl, string metadataFieldName)
        {
            using (ClientContext spContext = GetSpConnection_Ext())
            {
                var folder = spContext.Web.GetFolderByServerRelativeUrl(folderUrl);

                ListItem listItem = folder.ListItemAllFields;
                spContext.Load(listItem);
                spContext.ExecuteQuery();

                foreach (var field in listItem.FieldValues)
                {
                    if (metadataFieldName == field.Key)
                    {
                        return (string)field.Value;
                    }
                }
            }
            return string.Empty;
        }

        private FileMetadata GetFileMetadata(ClientContext spContext, Microsoft.SharePoint.Client.File file, DateTime modifiedDate)
        {
            FileMetadata fileMetadata = new FileMetadata();

            ListItem listItem = file.ListItemAllFields;
            spContext.Load(listItem);
            spContext.ExecuteQuery();

            foreach (var field in listItem.FieldValues)
            {
                if (GetMetadataFieldName("ModifiedBy") == field.Key)
                {
                    fileMetadata.ModifiedBy = (string)field.Value;
                }
                if (GetMetadataFieldName("CreatedBy") == field.Key)
                {
                    fileMetadata.CreatedBy = (string)field.Value;
                }
                if (GetMetadataFieldName("ModifiedOn") == field.Key)
                {
                    fileMetadata.ModifiedDate = (DateTime?)field.Value;
                }
            }

            Guid.TryParse(fileMetadata.CreatedBy, out Guid createdByUser);
            Guid.TryParse(fileMetadata.ModifiedBy, out Guid modifiedByUser);

            if (createdByUser != Guid.Empty || modifiedByUser != Guid.Empty)
            {
                if (fileMetadata.ModifiedDate?.Date != modifiedDate.Date)
                {
                    fileMetadata.ExternalModifiedBy = false;
                }
                else
                {
                    fileMetadata.ExternalModifiedBy = true;
                }
            }

            return fileMetadata;
        }

        private string GetMetadataFieldName(string key)
        {
            if (DocumentMetadata.MetadataConstants.TryGetValue(key, out var metadata))
            {
                return metadata[DocumentMetadata.FieldName];
            }

            return string.Empty;
        }

        private string GetCamlFieldName(string sortField)
        {
            switch (sortField)
            {
                case "DocumentName":
                    return "FileLeafRef";
                case "DocumentSizeInBytes":
                    return "File_x0020_Size";
                case "DocumentCreatedOnInUTC":
                    return "Created";
                case "DocumentModifiedOnInUTC":
                    return "Modified";
                case "DocumentCreatedBy":
                    return "Author";
                case "DocumentModifiedBy":
                    return "Editor";
                default:
                    return "Modified";
            }
        }

        private bool SearchByFileMetadata(ClientContext spContext, Microsoft.SharePoint.Client.File file, string searchText, out Dictionary<string, object> fileMetadata)
        {
            Dictionary<string, object> metaData = new Dictionary<string, object>();
            fileMetadata = metaData;

            ListItem listItem = file.ListItemAllFields;
            spContext.Load(listItem);
            spContext.ExecuteQuery();

            foreach (var field in listItem.FieldValues)
            {
                if (field.Value != null && field.Value.ToString().Contains(searchText))
                {
                    metaData.Add(field.Key, field.Value);
                    fileMetadata = metaData;

                    return true;
                }
            }

            return false;
        }

        private bool SearchByFileName(string fileName, string searchText)
        {
            return fileName.ToLower().Contains(searchText.ToLower());
        }

        private bool SearchInTextFile(Stream fileStream, string searchText)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string content = reader.ReadToEnd();
                    return content.Contains(searchText);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool SearchInWordDocument(Stream fileStream, string searchText)
        {
            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(fileStream, false))
                {
                    string content = wordDoc.MainDocumentPart.Document.Body.InnerText;
                    return content.Contains(searchText);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool SearchInExcelFile(Stream fileStream, string searchText)
        {
            try
            {
                using (SpreadsheetDocument excelDoc = SpreadsheetDocument.Open(fileStream, false))
                {
                    foreach (var sheet in excelDoc.WorkbookPart.WorksheetParts)
                    {
                        foreach (var row in sheet.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>())
                        {
                            foreach (var cell in row.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>())
                            {
                                string cellValue = GetCellValue(excelDoc, cell);
                                if (cellValue.Contains(searchText))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetCellValue(SpreadsheetDocument document, DocumentFormat.OpenXml.Spreadsheet.Cell cell)
        {
            string value = cell.InnerText;
            if (cell.DataType != null && cell.DataType.Value == DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString)
            {
                var stringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
                value = stringTable.ChildElements[int.Parse(value)].InnerText;
            }
            return value;
        }

        private bool SearchInPowerPoint(Stream fileStream, string searchText)
        {
            try
            {
                using (PresentationDocument pptDoc = PresentationDocument.Open(fileStream, false))
                {
                    foreach (var slidePart in pptDoc.PresentationPart.SlideParts)
                    {
                        string content = slidePart.Slide.InnerText;
                        if (content.Contains(searchText))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool SearchInPdfFile(Stream fileStream, string searchText)
        {
            try
            {
                using (var reader = new PdfReader(fileStream))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        string content = PdfTextExtractor.GetTextFromPage(reader, i);
                        if (content.Contains(searchText))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<GetDocumentListRsp> SPListItems(string libraryName, string list, bool isArchive, string user)
        {
            List<GetDocumentListRsp> listFiles = new List<GetDocumentListRsp>();
            using (ClientContext spContext = GetSpConnection())
            {
                ImpersonateUser(user, spContext);

                string folderPath = string.Join("/", spContext.Url.Trim('/'), libraryName.Trim('/'), list.Trim('/') + (isArchive ? "/Archive" : string.Empty));

                var fileslist = spContext.Web.GetFolderByServerRelativeUrl(folderPath).Files;
                spContext.Load(fileslist, inc => inc.Include(
                    i => i.Exists,
                    i => i.ListId,
                    i => i.UniqueId,
                    i => i.Name,
                    i => i.ServerRelativeUrl,
                    i => i.TimeCreated,
                    i => i.Length,
                    i => i.ModifiedBy,
                    i => i.Author
                ));

                spContext.ExecuteQuery();
                fileslist.Where(item => item != null && item.Exists).ToList().ForEach(file =>
                {
                    Uri currUrl = new Uri(spContext.Url);

                    listFiles.Add(new GetDocumentListRsp
                    {
                        DocumentName = file.Name,
                        DocumentCreatedOnInUTC = file.TimeCreated,
                        DocumentSizeInBytes = file.Length,
                        DocumentType = System.IO.Path.GetExtension(file.Name).ToLower(),
                        DocumentModifiedBy = file.ModifiedBy.Title,
                        DocumentCreatedBy = file.Author.Title
                        
                    });

                });
                return listFiles;
            }
        }

        public GetDocumentRsp SPGetItem(string libraryName, string DocName, string list, string user)
        {
            try
            {
                var docRes = new GetDocumentRsp();
                using (ClientContext spContext = GetSpConnection())
                {
                    ImpersonateUser(user, spContext);

                    try
                    {
                        string folderPath = string.Join("/", spContext.Url.Trim('/'), libraryName.Trim('/'), list.Trim('/') + (false ? "/Archive" : string.Empty));

                        var fileslist = spContext.Web.GetFolderByServerRelativeUrl(folderPath).Files;
                        spContext.Load(fileslist, inc => inc.Include(
                            i => i.Exists,
                            i => i.ListId,
                            i => i.UniqueId,
                            i => i.Name,
                            i => i.ServerRelativeUrl,
                            i => i.TimeCreated,
                            i => i.Length
                        ));

                        spContext.ExecuteQuery();

                        var file = fileslist.Where(item => item != null && item.Name == DocName && item.Exists).ToList().FirstOrDefault();

                        Uri currUrl = new Uri(spContext.Url);
                        string FullUrl = currUrl.AbsoluteUri.ToLower().Replace(currUrl.AbsolutePath, string.Empty) +
                                         file.ServerRelativeUrl;
                        string path = string.Empty;
                        try
                        {
                            path =
                                $"{currUrl.AbsoluteUri}/_layouts/15/WopiFrame.aspx?sourcedoc=%7B{file.UniqueId.ToString().ToUpper()}%7D&" +
                                $"file={HttpUtility.UrlPathEncode(file.Name).ToString().ToUpper()}&" +
                                "action=default&" +
                                "IsList=1&" +
                                $"ListId=%7B{file.ListId.ToString().ToUpper()}%7D";
                        }
                        catch (Exception ex)
                        {
                            new UserFriendlyException(ex.Message);
                        }

                        byte[] byteFile = DownloadFileContent(spContext, file.ServerRelativeUrl);

                        docRes.DocumentContent = byteFile;
                    }
                    catch (Exception ex)
                    {
                        throw new UserFriendlyException(ex.Message);
                    }

                    return docRes;
                }
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public GetDocumentRsp SPGetItem(string folderPath, string docName, string user)
        {
            try
            {
                var docRes = new GetDocumentRsp();
                using (ClientContext spContext = GetSpConnection())
                {
                    ImpersonateUser(user, spContext);

                    try
                    {
                        var fileslist = spContext.Web.GetFolderByServerRelativeUrl(folderPath).Files;
                        spContext.Load(fileslist, inc => inc.Include(
                            i => i.Exists,
                            i => i.ListId,
                            i => i.UniqueId,
                            i => i.Name,
                            i => i.ServerRelativeUrl,
                            i => i.TimeCreated,
                            i => i.Length
                        ));

                        spContext.ExecuteQuery();

                        var file = fileslist.Where(item => item != null && item.Name == docName && item.Exists).ToList().FirstOrDefault();

                        Uri currUrl = new Uri(spContext.Url);
                        string FullUrl = currUrl.AbsoluteUri.ToLower().Replace(currUrl.AbsolutePath, string.Empty) +
                                         file.ServerRelativeUrl;
                        string path = string.Empty;
                        try
                        {
                            path =
                                $"{currUrl.AbsoluteUri}/_layouts/15/WopiFrame.aspx?sourcedoc=%7B{file.UniqueId.ToString().ToUpper()}%7D&" +
                                $"file={HttpUtility.UrlPathEncode(file.Name).ToString().ToUpper()}&" +
                                "action=default&" +
                                "IsList=1&" +
                                $"ListId=%7B{file.ListId.ToString().ToUpper()}%7D";
                        }
                        catch (Exception ex)
                        {
                            new UserFriendlyException(ex.Message);
                        }

                        byte[] byteFile = DownloadFileContent(spContext, file.ServerRelativeUrl);

                        docRes.DocumentContent = byteFile;
                    }
                    catch (Exception ex)
                    {
                        throw new UserFriendlyException(ex.Message);
                    }

                    return docRes;
                }
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        private byte[] DownloadFileContent(ClientContext spContext, string fileUrl)
        {
            var file = spContext.Web.GetFileByServerRelativeUrl(fileUrl);
            spContext.Load(file);
            spContext.ExecuteQuery();

            var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(spContext, file.ServerRelativeUrl);
            using (var memoryStream = new MemoryStream())
            {
                fileInfo.Stream.CopyTo(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                return fileBytes;
            }
        }

        private void GetFileFromSPByPath(string filePath, out string fileName, out string base64content, string user)
        {
            try
            {

                using (ClientContext spContext = GetSpConnection())
                {

                    Microsoft.SharePoint.Client.File file =
                        spContext.Web.GetFileByServerRelativeUrl(new Uri(spContext.Url).AbsolutePath + filePath);
                    spContext.Load(file);
                    spContext.ExecuteQuery();

                    string fileRef = file.ServerRelativeUrl;

                    if (spContext.HasPendingRequest)
                        spContext.ExecuteQuery();

                    FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(spContext, fileRef);
                    byte[] bytes;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        fileInfo.Stream.CopyTo(memoryStream);
                        bytes = memoryStream.ToArray();
                    }

                    fileName = file.Name;
                    base64content = Convert.ToBase64String(bytes);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckIfPathExistsInSharePoint(string spPath)
        {
            try
            {
                ClientContext spContext = GetSpConnection();
                var Folder = spContext.Web.GetFolderByServerRelativeUrl(spPath);

                spContext.Load(Folder);

                spContext.ExecuteQuery();

                if (Folder.Exists)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool CheckIfPathExistsInSharePoint_Ext(string spPath)
        {
            try
            {
                ClientContext spContext = GetSpConnection_Ext();
                var Folder = spContext.Web.GetFolderByServerRelativeUrl(spPath);

                spContext.Load(Folder);

                spContext.ExecuteQuery();

                if (Folder.Exists)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public int SPListItemsCount(string libraryName, string spList)
        {
            using (ClientContext spContext = GetSpConnection())
            {
                var fileslist = spContext.Web
                    .GetFolderByServerRelativeUrl(string.Join("/", spContext.Url.Trim('/'), libraryName, spList)).Files;
                spContext.Load(fileslist, inc => inc.Include(
                    i => i.Exists,
                    i => i.ServerRelativeUrl
                ));

                spContext.ExecuteQuery();



                return fileslist.Count(listItem =>
                    (listItem != null && listItem.Exists) && listItem.ServerRelativeUrl.Contains(spList) &&
                    !listItem.ServerRelativeUrl.Contains("Archive"));
            }
        }
        public void AddFolderToSpRootLibrary(string entityLibrary, string folderName)
        {
            try
            {
                var siteName = ConfigurationManager.AppSettings["SPSiteName"];
                var SPRelativeUriPrefix = "sites/" + siteName;
                string relativePath = "/" + SPRelativeUriPrefix + "/" + $"{entityLibrary}/{folderName}";

                using (ClientContext spContext = GetSpConnection())
                {
                    if (CheckIfPathExistsInSharePoint(relativePath))
                        return;

                    List entityFolder = spContext.Web.GetList("/" + SPRelativeUriPrefix.Trim('/') + "/" + $"{entityLibrary}");

                    ListItem listItem = entityFolder.AddItem(new ListItemCreationInformation
                    {
                        UnderlyingObjectType = FileSystemObjectType.Folder,
                        LeafName = folderName
                    });

                    listItem["Title"] = folderName;
                    listItem.Update();
                    spContext.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "File Not Found.")
                    throw new Exception($"Invalid folder name format: [{folderName}]");
            }
        }

        public void UploadFileToSPLocation(string relativePath, string fileName, string base64File, string user)
        {
            try
            {
                using (ClientContext spContext = GetSpConnection())
                {
                    User CurrentUser = ImpersonateUser(user, spContext);

                    Folder targetFolder = spContext.Web.GetFolderByServerRelativeUrl(relativePath);
                    spContext.Load(targetFolder);
                    spContext.ExecuteQuery();

                    ImpersonateFolder(CurrentUser, targetFolder);

                    byte[] fileBytes = Convert.FromBase64String(base64File);
                    UploadLargeFile(spContext, targetFolder, fileName, fileBytes, CurrentUser);
                }
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public UploadFilesResponse UploadFiles(string relativePath, string fileName, string base64File, string user, Dictionary<string, object> Metadata)
        {
            UploadFilesResponse uploadFilesResponse = new UploadFilesResponse();

            uploadFilesResponse.Uploaded = true;
            uploadFilesResponse.Status = "Success";
            uploadFilesResponse.DocumentPath = relativePath;
            uploadFilesResponse.DocumentName = fileName;

            try
            {
                using (ClientContext spContext = GetSpConnection_Ext())
                {
                    spContext.Load(spContext.Web);
                    spContext.ExecuteQuery();
                    string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                    User CurrentUser = ImpersonateUser(user, spContext);

                    Folder targetFolder = spContext.Web.GetFolderByServerRelativeUrl(webRelativeUrl + relativePath);
                    spContext.Load(targetFolder);
                    spContext.ExecuteQuery();

                    ImpersonateFolder(CurrentUser, targetFolder);

                    byte[] fileBytes = Convert.FromBase64String(base64File);
                    UploadLargeFile(spContext, targetFolder, fileName, fileBytes, CurrentUser, Metadata);
                }
            }
            catch (Exception ex)
            {
                uploadFilesResponse.Uploaded = false;
                uploadFilesResponse.Status = ex.Message;
            }

            return uploadFilesResponse;
        }

        public void CheckFolderStructure(string folderName, string rootFolderPath)
        {
            var siteName = ConfigurationManager.AppSettings["SPSiteName_ext"];
            var SPRelativeUriPrefix = siteName;
            using (ClientContext spContext = GetSpConnection_Ext())
            {
                var isExists = CheckIfPathExistsInSharePoint_Ext($"/{SPRelativeUriPrefix}/{rootFolderPath}/{folderName}");
                if (!isExists)
                {
                    AddSubFolderWithMetaData(folderName, rootFolderPath, spContext, new Dictionary<string, object> { });
                }
            }
        }

        private void UploadLargeFile(ClientContext context, Folder targetFolder, string fileName, byte[] fileBytes, User currentUser, Dictionary<string, object> metadata = null)
        {
            const int chunkSize = 2 * 1024 * 1024; // 2 MB chunk size
            long fileSize = fileBytes.Length;
            ClientResult<long> bytesUploaded = null;
            Guid uploadId = Guid.NewGuid(); // Unique identifier for the upload session
            long fileOffset = 0;

            // Check if file already exists and get a unique name if necessary
            fileName = GetUniqueFileName(context, targetFolder, fileName);

            var fileCreationInfo = new FileCreationInformation
            {
                ContentStream = new MemoryStream(), // Initially empty; will be set per chunk
                Url = System.IO.Path.Combine(targetFolder.ServerRelativeUrl, fileName),
                Overwrite = true
            };

            // Create an empty file on the SharePoint library to start the upload
            var uploadFile = targetFolder.Files.Add(fileCreationInfo);
            context.Load(uploadFile);
            context.ExecuteQuery();

            while (fileOffset < fileSize)
            {
                int currentChunkSize = fileSize - fileOffset > chunkSize ? chunkSize : (int)(fileSize - fileOffset);
                byte[] chunk = new byte[currentChunkSize];
                Array.Copy(fileBytes, fileOffset, chunk, 0, currentChunkSize);

                using (MemoryStream ms = new MemoryStream(chunk))
                {
                    if (fileOffset == 0) // First chunk
                    {
                        bytesUploaded = uploadFile.StartUpload(uploadId, ms);
                    }
                    else
                    {
                        bytesUploaded = uploadFile.ContinueUpload(uploadId, fileOffset, ms);
                    }
                    context.ExecuteQuery();
                }

                fileOffset += currentChunkSize;

                // Check if this is the last chunk and call FinishUpload if so
                if (fileOffset == fileSize)
                {
                    using (MemoryStream ms = new MemoryStream(chunk))
                    {
                        uploadFile = uploadFile.FinishUpload(uploadId, fileOffset, ms);
                        context.ExecuteQuery();
                    }
                }
            }

            ImpersonateFile(currentUser, uploadFile);

            FillCustomFileMetadata(System.IO.Path.Combine(targetFolder.ServerRelativeUrl, fileName), metadata);

            Console.WriteLine("Upload complete for file: " + fileName);
        }

        private string GetUniqueFileName(ClientContext context, Folder targetFolder, string fileName)
        {
            context.Load(targetFolder.Files);
            context.ExecuteQuery();

            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
            string extension = System.IO.Path.GetExtension(fileName);
            int copyIndex = 1;

            while (targetFolder.Files.Any(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            {
                fileName = $"{fileNameWithoutExtension} (copy {copyIndex++}){extension}";
            }

            return fileName;
        }

        private void FillCustomFileMetadata(string serverRelativeUrl, Dictionary<string, object> metadata = null)
        {
            using (ClientContext spContext = GetSpConnection_Ext())
            {
                Microsoft.SharePoint.Client.File file = spContext.Web.GetFileByServerRelativeUrl(serverRelativeUrl.Replace("\\", "/"));

                spContext.Load(file);
                spContext.ExecuteQuery();

                if (metadata != null)
                {
                    ListItem listItem = file.ListItemAllFields;
                    spContext.Load(listItem);
                    spContext.ExecuteQuery();

                    foreach (var kvp in metadata)
                    {
                        listItem[kvp.Key] = kvp.Value;
                    }

                    listItem.Update();
                    spContext.ExecuteQuery();
                }
            }
        }

        public int SPDeleteFile(List<string> filesFullPath, string user, string crmSharepointUrl, string sharepointLibraryName)
        {
            int successCounter = 0;
            using (ClientContext spContext = GetSpConnection())
            {
                User thisUser = ImpersonateUser(user, spContext);

                filesFullPath.Where(fileFullPath => !string.IsNullOrWhiteSpace(fileFullPath)).ToList().ForEach(
                    fileFullPath =>
                    {
                        try
                        {
                            string relativeUrl;
                            Uri currUrl = new Uri(spContext.Url);
                            if (!string.IsNullOrEmpty(crmSharepointUrl))
                            {
                                var formattedurl = crmSharepointUrl + "/" + fileFullPath;
                                relativeUrl = currUrl.AbsolutePath + formattedurl.ToLower().Replace(currUrl.AbsoluteUri, string.Empty);
                            }
                            else
                            {
                                var formattedurl = string.Join("/", new[] { spContext.Url.TrimEnd('/'), SharePointDocumentLocation.RegardingObjectName, sharepointLibraryName, fileFullPath.TrimStart('/') });

                                relativeUrl = currUrl.AbsolutePath + formattedurl.ToLower().Replace(currUrl.AbsoluteUri, string.Empty);

                            }
                            var file = spContext.Web.GetFileByServerRelativeUrl(relativeUrl);
                            ImpersonateFile(thisUser, file);

                            spContext.Load(file, f => f.Exists);
                            spContext.ExecuteQuery();

                            if (file.Exists)
                            {
                                file.DeleteObject();
                                spContext.ExecuteQuery();
                                successCounter++;
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    });
            }

            return successCounter;
        }

        private User ImpersonateUser(string strUserDomainName, ClientContext spContext)
        {
            User theUser = spContext.Web.EnsureUser(strUserDomainName);
            spContext.Load(theUser);
            spContext.ExecuteQuery();
            return theUser;
        }
        private void ImpersonateFolder(User CurrentUser, params Microsoft.SharePoint.Client.Folder[] targetFolder)
        {
            targetFolder.ToList().ForEach(folder =>
            {
                var listItemAllFields = folder.ListItemAllFields;
                listItemAllFields["Author"] = CurrentUser;
                listItemAllFields["Editor"] = CurrentUser;
                listItemAllFields.Update();
            });
        }
        private void ImpersonateFile(User CurrentUser, params Microsoft.SharePoint.Client.File[] uploadFile)
        {
            uploadFile.ToList().ForEach(file =>
            {
                ListItem listItemAllFields = file.ListItemAllFields;
                listItemAllFields["Author"] = CurrentUser;
                listItemAllFields["Editor"] = CurrentUser;
                file.ListItemAllFields.Update();
            });
        }

        public void GetFolderPathAndItemCount(string fullRelativeUrl, out string folderPath)
        {
            using (ClientContext spContext = GetSpConnection())
            {
                Uri currUrl = new Uri(spContext.Url);
                folderPath = $"{currUrl.AbsoluteUri.ToLower()}/{fullRelativeUrl}";
            }
        }
        public List<DeleteFileResponse> DeleteDocument(List<DeleteFileRequest> deleteFileRequest)
        {
            List<DeleteFileResponse> deletedFiles = new List<DeleteFileResponse>();
            using (ClientContext spContext = GetSpConnection_Ext())
            {

                foreach (var deleteFile in deleteFileRequest)
                {
                    bool deleted = false;
                    string status = "Success";
                    Microsoft.SharePoint.Client.File file = null;

                    try
                    {
                        spContext.Load(spContext.Web);
                        spContext.ExecuteQuery();
                        string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                        deleteFile.SourceFilePath = webRelativeUrl + deleteFile.SourceFilePath;

                        file = spContext.Web.GetFileByServerRelativeUrl(deleteFile.SourceFilePath);

                        spContext.Load(file, f => f.Exists);
                        spContext.ExecuteQuery();

                        if (file.Exists)
                        {
                            file.DeleteObject();
                            spContext.ExecuteQuery();
                        }
                        else
                        {
                            status = $"Source file does not exist: {deleteFile.SourceFilePath}";
                            goto FillResponse;
                        }

                        deleted = true;
                        goto FillResponse;
                    }
                    catch (Exception ex)
                    {
                        status = $"Error deleting file {deleteFile.SourceFilePath}: {ex.Message}";

                        goto FillResponse;
                    }

                FillResponse:
                    deletedFiles.Add(new DeleteFileResponse
                    {
                        DocumentPath = deleteFile.SourceFilePath,
                        Deleted = deleted,
                        Status = status
                    });
                }
            }

            return deletedFiles;
        }

        public List<RenameFilesResponse> RenameDocuments(List<RenameFilesDto> renameFilesDtos, Guid contactId)
        {
            List<RenameFilesResponse> renamedFiles = new List<RenameFilesResponse>();

            using (ClientContext spContext = GetSpConnection_Ext())
            {
                foreach (var renameFilesDto in renameFilesDtos)
                {
                    bool renamed = false;
                    string status = "Success";
                    string destinationPath = string.Empty;
                    Microsoft.SharePoint.Client.File file = null;

                    renameFilesDto.SourceFilePath.Replace(" ", "%20");

                    try
                    {
                        spContext.Load(spContext.Web);
                        spContext.ExecuteQuery();
                        string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                        renameFilesDto.SourceFilePath = webRelativeUrl + renameFilesDto.SourceFilePath;

                        file = spContext.Web.GetFileByServerRelativeUrl(renameFilesDto.SourceFilePath);
                        spContext.Load(file);
                        spContext.ExecuteQuery();

                        renameFilesDto.NewFileName = renameFilesDto.NewFileName + System.IO.Path.GetExtension(file.Name).ToLower();

                        if (!file.Exists)
                        {
                            status = $"Source file does not exist: {renameFilesDto.SourceFilePath}";
                            goto FillResponse;
                        }

                        destinationPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(renameFilesDto.SourceFilePath), renameFilesDto.NewFileName);
                        destinationPath = destinationPath.Replace("\\", "/");

                        var folder = spContext.Web.GetFolderByServerRelativeUrl(System.IO.Path.GetDirectoryName(renameFilesDto.SourceFilePath));
                        var files = folder.Files;
                        spContext.Load(files);
                        spContext.ExecuteQuery();

                        if (files.Any(f => f.Name.Equals(renameFilesDto.NewFileName, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (renameFilesDto.OverriddenAction == (int)FileOverriddenAction.NoAction)
                            {
                                status = $"ERR01::File with name {renameFilesDto.NewFileName} already exists in the folder: {System.IO.Path.GetDirectoryName(renameFilesDto.SourceFilePath)}";
                                goto FillResponse;
                            }
                            else if (renameFilesDto.OverriddenAction == (int)FileOverriddenAction.KeepBoth)
                            {
                                string fileName = GetUniqueFileName(spContext, folder, renameFilesDto.NewFileName);

                                destinationPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(renameFilesDto.SourceFilePath), fileName);
                                destinationPath = destinationPath.Replace("\\", "/");
                            }
                        }

                        file.MoveTo(destinationPath, MoveOperations.Overwrite);
                        spContext.ExecuteQuery();

                        var metadataDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy")?.FieldName, contactId },
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn")?.FieldName, DateTime.Now }
                                    };

                        FillCustomFileMetadata(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(renameFilesDto.SourceFilePath), renameFilesDto.NewFileName), metadataDic);

                        renamed = true;
                        goto FillResponse;
                    }
                    catch (Exception ex)
                    {
                        status = $"Error renaming file {renameFilesDto.SourceFilePath}: {ex.Message}";

                        goto FillResponse;
                    }

                FillResponse:
                    renamedFiles.Add(new RenameFilesResponse
                    {
                        DocumentName = renameFilesDto.NewFileName,
                        DocumentPath = destinationPath,
                        DocumentCreatedOnInUTC = file != null ? file.TimeCreated : DateTime.MinValue,
                        DocumentSizeInBytes = file != null ? file.Length : 0,
                        Renamed = renamed,
                        Status = status
                    });
                }
            }
            return renamedFiles;
        }

        public List<MoveFilesResponse> MoveDocuments(List<MoveFilesDto> moveFilesDtos, Guid contactId)
        {
            List<MoveFilesResponse> movedFiles = new List<MoveFilesResponse>();

            using (ClientContext spContext = GetSpConnection_Ext())
            {
                foreach (var moveFilesDto in moveFilesDtos)
                {
                    bool moved = false;
                    string status = "Success";
                    string destinationPath = string.Empty;
                    Microsoft.SharePoint.Client.File file = null;
                    Microsoft.SharePoint.Client.Folder folder = null;

                    moveFilesDto.SourceFilePath.Replace(" ", "%20");
                    moveFilesDto.DestinationFilePath.Replace(" ", "%20");

                    try
                    {
                        spContext.Load(spContext.Web);
                        spContext.ExecuteQuery();
                        string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                        moveFilesDto.SourceFilePath = webRelativeUrl + moveFilesDto.SourceFilePath;
                        moveFilesDto.DestinationFilePath = webRelativeUrl + moveFilesDto.DestinationFilePath;

                        file = spContext.Web.GetFileByServerRelativeUrl(moveFilesDto.SourceFilePath);
                        spContext.Load(file);
                        spContext.ExecuteQuery();

                        if (!file.Exists)
                        {
                            status = $"Source file does not exist: {moveFilesDto.SourceFilePath}";
                            goto FillResponse;
                        }

                        folder = spContext.Web.GetFolderByServerRelativeUrl(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath.Replace("\\", "/")));
                        spContext.Load(folder);
                        spContext.ExecuteQuery();

                        if (!folder.Exists)
                        {
                            status = $"Destination Folder does not exist: {moveFilesDto.DestinationFilePath}";
                            goto FillResponse;
                        }

                        var destinationFolder = spContext.Web.GetFolderByServerRelativeUrl(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath));
                        var destinationFiles = destinationFolder.Files;
                        spContext.Load(destinationFiles);
                        spContext.ExecuteQuery();

                        if (destinationFiles.Any(f => f.ServerRelativeUrl.Equals(moveFilesDto.DestinationFilePath, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (moveFilesDto.OverriddenAction == (int)FileOverriddenAction.NoAction)
                            {
                                status = $"ERR01::Destination file already exists: {moveFilesDto.DestinationFilePath}";
                                goto FillResponse;
                            }
                            else if (moveFilesDto.OverriddenAction == (int)FileOverriddenAction.KeepBoth)
                            {
                                var segments = moveFilesDto.DestinationFilePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                string fileName = GetUniqueFileName(spContext, folder, segments[segments.Length - 1]);

                                destinationPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath), fileName);
                                destinationPath = destinationPath.Replace("\\", "/");
                                moveFilesDto.DestinationFilePath = destinationPath;
                            }
                        }

                        file.MoveTo(moveFilesDto.DestinationFilePath, MoveOperations.Overwrite);
                        spContext.ExecuteQuery();

                        var metadataDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy")?.FieldName, contactId },
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn")?.FieldName, DateTime.Now }
                                    };

                        FillCustomFileMetadata(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath), System.IO.Path.GetFileName(moveFilesDto.DestinationFilePath)), metadataDic);

                        moved = true;
                        goto FillResponse;
                    }
                    catch (Exception ex)
                    {
                        status = $"Error moving file {moveFilesDto.SourceFilePath}: {ex.Message}";
                        goto FillResponse;
                    }

                FillResponse:
                    movedFiles.Add(new MoveFilesResponse
                    {
                        DocumentName = System.IO.Path.GetFileName(moveFilesDto.DestinationFilePath),
                        DocumentPath = moveFilesDto.DestinationFilePath,
                        DocumentCreatedOnInUTC = file != null ? file.TimeCreated : DateTime.MinValue,
                        DocumentSizeInBytes = file != null ? file.Length : 0,
                        Moved = moved,
                        Status = status
                    });
                }
            }
            return movedFiles;
        }

        public List<CopyFileResponse> CopyDocuments(List<CopyFileDto> copyFileRequest, Guid contactId)
        {
            List<CopyFileResponse> copiedFiles = new List<CopyFileResponse>();

            using (ClientContext spContext = GetSpConnection_Ext())
            {
                foreach (var moveFilesDto in copyFileRequest)
                {
                    bool copied = false;
                    string status = "Success";
                    string destinationPath = string.Empty;
                    Microsoft.SharePoint.Client.File file = null;
                    Microsoft.SharePoint.Client.Folder folder = null;

                    try
                    {
                        spContext.Load(spContext.Web);
                        spContext.ExecuteQuery();
                        string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                        moveFilesDto.SourceFilePath = webRelativeUrl + moveFilesDto.SourceFilePath;
                        moveFilesDto.DestinationFilePath = webRelativeUrl + moveFilesDto.DestinationFilePath;

                        file = spContext.Web.GetFileByServerRelativeUrl(moveFilesDto.SourceFilePath);
                        spContext.Load(file);
                        spContext.ExecuteQuery();

                        if (!file.Exists)
                        {
                            status = $"Source file does not exist: {moveFilesDto.SourceFilePath}";
                            goto FillResponse;
                        }

                        folder = spContext.Web.GetFolderByServerRelativeUrl(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath.Replace("\\", "/")));
                        spContext.Load(folder);
                        spContext.ExecuteQuery();

                        if (!folder.Exists)
                        {
                            status = $"Destination Folder does not exist: {moveFilesDto.DestinationFilePath}";
                            goto FillResponse;
                        }

                        var destinationFolder = spContext.Web.GetFolderByServerRelativeUrl(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath));
                        var destinationFiles = destinationFolder.Files;
                        spContext.Load(destinationFiles);
                        spContext.ExecuteQuery();

                        if (destinationFiles.Any(f => f.ServerRelativeUrl.Equals(moveFilesDto.DestinationFilePath, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (moveFilesDto.OverriddenAction == (int)FileOverriddenAction.NoAction)
                            {
                                status = $"ERR01::Destination file already exists: {moveFilesDto.DestinationFilePath}";
                                goto FillResponse;
                            }
                            else if (moveFilesDto.OverriddenAction == (int)FileOverriddenAction.KeepBoth)
                            {
                                var segments = moveFilesDto.DestinationFilePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                string fileName = GetUniqueFileName(spContext, folder, segments[segments.Length - 1]);

                                destinationPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath), fileName);
                                destinationPath = destinationPath.Replace("\\", "/");
                                moveFilesDto.DestinationFilePath = destinationPath;
                            }
                        }

                        file.CopyTo(moveFilesDto.DestinationFilePath, true);
                        spContext.ExecuteQuery();

                        var metadataDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy")?.FieldName, contactId },
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn")?.FieldName, DateTime.Now }
                                    };

                        FillCustomFileMetadata(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(moveFilesDto.DestinationFilePath), System.IO.Path.GetFileName(moveFilesDto.DestinationFilePath)), metadataDic);

                        copied = true;
                        goto FillResponse;
                    }
                    catch (Exception ex)
                    {
                        status = $"Error copying file {moveFilesDto.SourceFilePath}: {ex.Message}";
                        goto FillResponse;
                    }

                FillResponse:
                    copiedFiles.Add(new CopyFileResponse
                    {
                        DocumentName = System.IO.Path.GetFileName(moveFilesDto.DestinationFilePath),
                        DocumentPath = moveFilesDto.DestinationFilePath,
                        DocumentCreatedOnInUTC = file != null ? file.TimeCreated : DateTime.MinValue,
                        DocumentSizeInBytes = file != null ? file.Length : 0,
                        Copied = copied,
                        Status = status
                    });
                }
            }
            return copiedFiles;
        }

        public string ShareFileOrFolder(string fileUrl, string userEmail, int role)
        {
            try
            {
                using (ClientContext clientContext = GetSpConnection_Ext())
                {
                    Microsoft.SharePoint.Client.File file = null;
                    clientContext.Load(clientContext.Web);
                    clientContext.ExecuteQuery();
                    string webRelativeUrl = clientContext.Web.ServerRelativeUrl.TrimEnd('/');

                    file = clientContext.Web.GetFileByServerRelativeUrl(webRelativeUrl + fileUrl);
                    clientContext.Load(file, f => f.ListItemAllFields, f => f.ListItemAllFields.HasUniqueRoleAssignments);
                    clientContext.ExecuteQuery();

                    // Break permission inheritance if necessary
                    if (!file.ListItemAllFields.HasUniqueRoleAssignments)
                    {
                        // Break permission inheritance (copy current permissions)
                        file.ListItemAllFields.BreakRoleInheritance(true, true);
                        file.ListItemAllFields.Update();
                        clientContext.ExecuteQuery();
                    }

                    // Get the user
                    User user = clientContext.Web.EnsureUser(userEmail);
                    clientContext.Load(user);
                    clientContext.ExecuteQuery();

                    // Grant permissions to the user
                    RoleDefinitionBindingCollection roleDefinitionBindingCollection = new RoleDefinitionBindingCollection(clientContext);
                    RoleDefinition roleDefinition = clientContext.Web.RoleDefinitions.GetByType((RoleType)role);
                    roleDefinitionBindingCollection.Add(roleDefinition);

                    // Assign Role to the user on the file
                    file.ListItemAllFields.RoleAssignments.Add(user, roleDefinitionBindingCollection);
                    file.ListItemAllFields.Update();
                    clientContext.ExecuteQuery();

                    return ($"Successfully shared {fileUrl} with {userEmail}.");
                }
            }
            catch (Exception ex)
            {
                return ("Error while sharing the file: " + ex.Message);
            }
        }

        public string ShareFileOrFolderWithGroup(string fileUrl, string groupName, int role)
        {
            try
            {
                using (ClientContext clientContext = GetSpConnection_Ext())
                {
                    Microsoft.SharePoint.Client.File file = null;
                    clientContext.Load(clientContext.Web);
                    clientContext.ExecuteQuery();
                    string webRelativeUrl = clientContext.Web.ServerRelativeUrl.TrimEnd('/');

                    file = clientContext.Web.GetFileByServerRelativeUrl(webRelativeUrl + fileUrl);
                    clientContext.Load(file, f => f.ListItemAllFields, f => f.ListItemAllFields.HasUniqueRoleAssignments);
                    clientContext.ExecuteQuery();

                    // Break permission inheritance if necessary
                    if (!file.ListItemAllFields.HasUniqueRoleAssignments)
                    {
                        // Break permission inheritance (copy current permissions)
                        file.ListItemAllFields.BreakRoleInheritance(true, true);
                        file.ListItemAllFields.Update();
                        clientContext.ExecuteQuery();
                    }

                    // Get the SharePoint group
                    Group group = clientContext.Web.SiteGroups.GetByName(groupName);
                    clientContext.Load(group);
                    clientContext.ExecuteQuery();

                    // Grant permissions to the group
                    RoleDefinitionBindingCollection roleDefinitionBindingCollection = new RoleDefinitionBindingCollection(clientContext);
                    RoleDefinition roleDefinition = clientContext.Web.RoleDefinitions.GetByType((RoleType)role);
                    roleDefinitionBindingCollection.Add(roleDefinition);

                    // Assign Role to the group on the file
                    file.ListItemAllFields.RoleAssignments.Add(group, roleDefinitionBindingCollection);
                    file.ListItemAllFields.Update();
                    clientContext.ExecuteQuery();

                    return ($"Successfully shared {fileUrl} with the group '{groupName}'.");
                }
            }
            catch (Exception ex)
            {
                return ("Error while sharing the file with the group: " + ex.Message);
            }
        }
        public byte[] DowloadDocument(string sourceFilePath)
        {
            using (ClientContext spContext = GetSpConnection_Ext())
            {
                spContext.Load(spContext.Web);
                spContext.ExecuteQuery();
                string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');
                string fileUrl = $"{webRelativeUrl}{sourceFilePath}";
                byte[] byteFile = DownloadFileContent(spContext, fileUrl);
                return byteFile;
            }
        }

        private void AddSubFolderWithMetaData(string folderName, string compFolderPath, ClientContext spContext, Dictionary<string, object> metaDataDic)
        {
            // Load the "comp" folder
            Folder compFolder = spContext.Web.GetFolderByServerRelativeUrl(compFolderPath);
            spContext.Load(compFolder);
            spContext.ExecuteQuery();

            // Create a new folder inside the "comp" folder
            Folder newFolder = compFolder.Folders.Add($"{folderName}");
            spContext.Load(newFolder);
            newFolder.Update();
            spContext.ExecuteQuery();

            // Get the ListItem associated with the new folder
            ListItem folderItem = newFolder.ListItemAllFields;

            // Set metadata on the folder
            foreach (var item in metaDataDic)
            {
                folderItem[item.Key] = item.Value;
            }
            folderItem.Update();

            // Commit the changes to SharePoint
            spContext.ExecuteQuery();
        }
        public void AddSubFolderWithMetaData(string folderName, string compFolderPath, Dictionary<string, object> metaDataDic)
        {
            using (ClientContext spContext = GetSpConnection_Ext())
            {
                // Load the "comp" folder
                Folder compFolder = spContext.Web.GetFolderByServerRelativeUrl(compFolderPath);
                spContext.Load(compFolder);
                spContext.ExecuteQuery();

                // Create a new folder inside the "comp" folder
                Folder newFolder = compFolder.Folders.Add($"{folderName}");
                spContext.Load(newFolder);
                newFolder.Update();
                spContext.ExecuteQuery();

                // Get the ListItem associated with the new folder
                ListItem folderItem = newFolder.ListItemAllFields;

                // Set metadata on the folder
                foreach (var item in metaDataDic)
                {
                    folderItem[item.Key] = item.Value;
                }
                folderItem.Update();

                // Commit the changes to SharePoint
                spContext.ExecuteQuery();
            }
        }

        private void EnsureCustomFieldsExist(ClientContext spContext, string listTitle, List<DocumentMetadataDto> fields)
        {
            try
            {
                // Load the list
                List list = spContext.Web.Lists.GetByTitle(listTitle);
                spContext.Load(list);
                spContext.Load(list.Fields);
                spContext.ExecuteQuery();

                // Determine which fields already exist
                var existingFieldNames = list.Fields.Select(f => f.InternalName).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

                // Add missing fields
                foreach (var field in fields)
                {
                    if (!existingFieldNames.Contains(field.FieldName))
                    {
                        string fieldSchema = $"<Field Type='{field.FieldType}' Name='{field.FieldName}' DisplayName='{field.DisplayName}' />";
                        list.Fields.AddFieldAsXml(fieldSchema, true, AddFieldOptions.DefaultValue);
                    }
                }

                // Execute all changes in a single query
                spContext.ExecuteQuery();
            }
            catch (ServerException)
            {
                throw new Exception($"The list '{listTitle}' does not exist at the site URL '{spContext.Web.Url}'. Please check the list name and site URL.");
            }
        }

        public void EnsureAllDataCollCustomFieldExists(string companyLibDisplayName, string contactLibDisplayName)
        {
            using (ClientContext spContext = GetSpConnection_Ext())
            {
                // Define fields for both libraries
                var companyFields = new List<DocumentMetadataDto>
        {
            DocumentMetadata.GetDocumentMetadataByKey("CompanyId"),
            DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy"),
            DocumentMetadata.GetDocumentMetadataByKey("CreatedBy"),
            DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn"),
            DocumentMetadata.GetDocumentMetadataByKey("SubFolderId"),
            DocumentMetadata.GetDocumentMetadataByKey("FolderId")
        };

                var contactFields = new List<DocumentMetadataDto>
        {
            DocumentMetadata.GetDocumentMetadataByKey("ContactId"),
            DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy"),
            DocumentMetadata.GetDocumentMetadataByKey("CreatedBy"),
            DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn"),
            DocumentMetadata.GetDocumentMetadataByKey("SubFolderId"),
            DocumentMetadata.GetDocumentMetadataByKey("FolderId")
        };

                // Process fields in bulk for both libraries
                EnsureCustomFieldsExist(spContext, companyLibDisplayName, companyFields);
                EnsureCustomFieldsExist(spContext, contactLibDisplayName, contactFields);
            }
        }

        public void EnsureAllKnowledgeHubCustomFieldExists(string knowledgeHubLibrary)
        {
            using (ClientContext spContext = GetSpConnection_Ext())
            {

                var knowledgeHubFields = new List<DocumentMetadataDto>
        {
            DocumentMetadata.GetDocumentMetadataByKey("CompanyId"),
            DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy"),
            DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn"),
            DocumentMetadata.GetDocumentMetadataByKey("CreatedBy")
        };
                EnsureCustomFieldsExist(spContext, knowledgeHubLibrary, knowledgeHubFields);
            }
        }

        public string GetFileTypeByFileUrl(string fileUrl)
        {
            using (ClientContext spContext = GetSpConnection_Ext())
            {
                spContext.Load(spContext.Web);
                spContext.ExecuteQuery();
                string webRelativeUrl = spContext.Web.ServerRelativeUrl.TrimEnd('/');

                string fullFileUrl = $"{webRelativeUrl}{fileUrl}";
                var file = spContext.Web.GetFileByServerRelativeUrl(fullFileUrl);
                spContext.Load(file);
                spContext.ExecuteQuery();

                string fileExtension = System.IO.Path.GetExtension(file.Name).TrimStart('.').ToLower();

                var mimeType = GetMimeType(fileExtension);

                return mimeType;
            }
        }
        private string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case "doc": return "application/msword";
                case "docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "pdf": return "application/pdf";
                case "ppt": return "application/vnd.ms-powerpoint";
                case "pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "xls": return "application/vnd.ms-excel";
                case "xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "jpeg": return "image/jpeg";
                case "jpg": return "image/jpeg";
                case "png": return "image/png";
                case "svg": return "image/svg+xml";
                case "dwg": return "application/acad";
                case "mp4": return "video/mp4";
                case "txt": return "text/plain";
                default: return "application/octet-stream"; // Fallback MIME type
            }
        }
        public AttendeeEventDetailsDto RetrieveAttendeesDetailsByRefId(string refId, string listName)
        {
            using (ClientContext clientContext = GetSpConnection_Private())
            {

                List list = clientContext.Web.Lists.GetByTitle(listName);

                // Build a CAML query to filter by RefID
                CamlQuery query = new CamlQuery
                {
                    ViewXml = $@"
                    <View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='RefID' />
                                    <Value Type='Text'>{refId}</Value>
                                </Eq>
                            </Where>
                        </Query>
                    </View>"
                };

                ListItemCollection items = list.GetItems(query);
                clientContext.Load(items,
                    itemsCollection => itemsCollection.Include(
                        item => item["ID"],
                        item => item["RSVP"],
                        item => item["CheckedIn"],
                        item => item["Table"],
                        item => item["Seat"],
                        item => item["RefID"],
                        item => item["Attendee"],
                        item => item["Event"],
                        item => item["GuestName"],
                        item => item["GuestRole"]
                    ));
                clientContext.ExecuteQuery();

                if (items.Count > 0)
                {
                    AttendeeEventDetailsDto attendeeEventDetailsDto = new AttendeeEventDetailsDto();
                    var item = items[0]; // Get the first matching item

                    attendeeEventDetailsDto.Id = item["ID"]?.ToString();
                    attendeeEventDetailsDto.RSVP = item["RSVP"]?.ToString();
                    attendeeEventDetailsDto.CheckedIn = item["CheckedIn"]?.ToString();
                    attendeeEventDetailsDto.Table = item["Table"]?.ToString();
                    attendeeEventDetailsDto.Seat = item["Seat"]?.ToString();
                    attendeeEventDetailsDto.RefId = item["RefID"].ToString();
                    attendeeEventDetailsDto.GuestName = item["GuestName"]?.ToString();
                    attendeeEventDetailsDto.GuestRole = item["GuestRole"]?.ToString();

                    // For lookup fields, retrieve the values
                    var attendeeField = item["Attendee"] as FieldLookupValue;
                    if (attendeeField != null)
                    {
                        attendeeEventDetailsDto.Attendee = new FieldLookupValueDto { Id = attendeeField.LookupId.ToString(), Name = attendeeField.LookupValue };
                    }

                    var eventField = item["Event"] as FieldLookupValue;
                    if (eventField != null)
                    {
                        var eventDetails = new EventDetailsDto
                        {
                            Id = eventField.LookupId.ToString(),
                            Name = eventField.LookupValue
                        };

                        int eventId = eventField.LookupId;

                        // Retrieve the additional Event details
                        List eventsList = clientContext.Web.Lists.GetByTitle("Events");

                        ListItem eventItem = eventsList.GetItemById(eventId);
                        clientContext.Load(eventItem,
                            e => e["DressCode"],
                            e => e["EventTime"],
                            e => e["Date"],
                            e => e["Location"],
                            e => e["ContactNumber"],
                            e => e["ShowSeatingInfo"],
                            e => e["Hide"]);
                        try
                        {
                            clientContext.ExecuteQuery();

                            eventDetails.DressCode = eventItem["DressCode"]?.ToString();

                            eventDetails.Location = eventItem["Location"]?.ToString();

                            eventDetails.ContactNumber = eventItem["ContactNumber"]?.ToString();

                            eventDetails.EventTime = eventItem["EventTime"]?.ToString();

                            if (eventItem["Date"] != null)
                            {
                                DateTime fullDateTime = Convert.ToDateTime(eventItem["Date"]);
                                eventDetails.EventDate = fullDateTime.ToString("dd/MM/yyyy");
                            }


                            if (eventItem["Hide"] != null)
                            {
                                    eventDetails.Hide = (bool)eventItem["Hide"];
                              
                            }

                            if (eventItem["ShowSeatingInfo"] != null)
                            {
                                    eventDetails.ShowSeatingInfo = (bool)eventItem["ShowSeatingInfo"];
                            }

                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error retrieving event details: {ex.Message}");
                        }

                        // Assign the completed EventDetailsDto to the Event property
                        attendeeEventDetailsDto.Event = eventDetails;
                    }
                    return attendeeEventDetailsDto;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool UpdateAttendeesDetails(string listName, string refId, string newRSVPValue, string guestName = null, string guestRole = null)
        {
            using (ClientContext clientContext = GetSpConnection_Private())
            {
                List list = clientContext.Web.Lists.GetByTitle(listName);

                // Build a CAML query to find the item by RefID
                CamlQuery query = new CamlQuery
                {
                    ViewXml = $@"
                    <View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='RefID' />
                                    <Value Type='Text'>{refId}</Value>
                                </Eq>
                            </Where>
                        </Query>
                    </View>"
                };

                // Execute the query
                ListItemCollection items = list.GetItems(query);
                clientContext.Load(items);
                clientContext.ExecuteQuery();

                if (items.Count > 0)
                {
                    ListItem item = items[0]; // Get the first matching item

                    // Update the RSVP field
                    item["RSVP"] = newRSVPValue;

                    // Update the GuestName and GuestRole field if provided
                    if (guestName != null)
                    {
                        item["GuestName"] = guestName;
                    }

                    if (guestRole != null)
                    {
                        item["GuestRole"] = guestRole;
                    }

                    item.Update();

                    // Save changes to SharePoint
                    clientContext.ExecuteQuery();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

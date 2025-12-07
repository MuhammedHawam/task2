using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PIF.EBP.Core.FileScanning;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using PIF.EBP.Core.FileScanning.DTOs;
using PIF.EBP.Core.FileManagement.DTOs;
using System.Text;
using PIF.EBP.Core.Utilities;

namespace PIF.EBP.Integrations.FileScanning.Implementation
{
    public class OpswatFileScanningService : IExternalFileScanningService
    {
        private OpswatBaseOptions _options;
        private HttpClient _client;

        public OpswatFileScanningService()
        {

            var jsonConfiguration = ConfigurationManager.AppSettings["OpswatConfig"];
            _options = JsonConvert.DeserializeObject<OpswatBaseOptions>(jsonConfiguration);
        }

        public async Task<string> AnalyzeFile(byte[] fileContent, string fileName, object metaData)
        {
            string dataId = string.Empty;
            var headers = new Dictionary<string, string>
            {
                ["apikey"] = _options.ApiKey,
                ["filename"] = fileName,
                ["sanitizedurl"] = _options.SanitizedUrl,
                ["metadata"] = metaData != null ? JsonConvert.SerializeObject(metaData) : null,
                ["rule"] = _options.RuleName
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_options.Url + "file")
            };

            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            request.Content = new ByteArrayContent(fileContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                _client = new HttpClient();
                _client.DefaultRequestHeaders.ConnectionClose = false;

                try
                {
                    var response = await _client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(responseContent);
                    dataId = (string)jsonObject["data_id"];
                }
                catch (Exception ex)
                {
                    return JsonConvert.SerializeObject(ex);
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }

            return dataId;
        }

        public async Task<UploadDocumentsDto> GetFileResult(string content, string dataId)
        {
            var jsonConfiguration = ConfigurationManager.AppSettings["OpswatConfig"];
            _options = JsonConvert.DeserializeObject<OpswatBaseOptions>(jsonConfiguration);

            UploadDocumentsDto uploadDocumentsDto = new UploadDocumentsDto();

            try
            {
                var headers = new Dictionary<string, string>
                {
                    ["apikey"] = _options.ApiKey
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(_options.Url + "file/" + dataId)
                };

                try
                {
                    HttpResponseMessage response = null;

                    using (_client = new HttpClient())
                    {
                        _client.DefaultRequestHeaders.Add("apikey", _options.ApiKey);
                        response = await _client.GetAsync(request.RequestUri);
                    }

                    response.EnsureSuccessStatusCode();
                    string responseContent = await response.Content.ReadAsStringAsync();

                    FileScanningResultDto fileScanningResultDto = JsonConvert.DeserializeObject<FileScanningResultDto>(responseContent);
                    if (fileScanningResultDto != null && fileScanningResultDto.scan_results != null && fileScanningResultDto.file_info != null)
                    {
                        int? fileResult = fileScanningResultDto.scan_results?.scan_all_result_i;
                        string fileName = fileScanningResultDto.file_info?.display_name;

                        if (fileResult.HasValue && !string.IsNullOrEmpty(fileName))
                        {
                            if (string.IsNullOrEmpty(fileScanningResultDto.metadata.FeedbackId))
                            {
                                var metadataDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocumentMetadataByKey("CompanyId")?.FieldName, fileScanningResultDto.metadata.CompanyId},
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy")?.FieldName, fileScanningResultDto.metadata.modified_by_contact_id },
                                        { DocumentMetadata.GetDocumentMetadataByKey("CreatedBy")?.FieldName, fileScanningResultDto.metadata.created_by_contact_Id },
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn")?.FieldName ,fileScanningResultDto.metadata.modified_by_contact_DateTime}
                                    };

                                uploadDocumentsDto.FileMetadata = metadataDic;
                                if (!string.IsNullOrEmpty(fileScanningResultDto.metadata.RegardingObjectId))
                                {
                                    uploadDocumentsDto.RegardingObjectId = new Guid(fileScanningResultDto.metadata.RegardingObjectId);
                                }
                                uploadDocumentsDto.CompanyId = fileScanningResultDto.metadata.CompanyId;
                                uploadDocumentsDto.ContactId = fileScanningResultDto.metadata.modified_by_contact_id;
                                uploadDocumentsDto.TargetFolderURL = fileScanningResultDto.metadata.TargetFolderURL;
                                uploadDocumentsDto.FileScanningResult = fileResult.Value;
                                if (!string.IsNullOrEmpty(fileScanningResultDto.metadata.KnowledgeItemId))
                                {
                                    uploadDocumentsDto.KnowledgeItemId = new Guid(fileScanningResultDto.metadata.KnowledgeItemId);
                                }

                                bool.TryParse(fileScanningResultDto.metadata.IsPifNotify, out bool IsNotify);
                                uploadDocumentsDto.IsPifNotify = IsNotify;
                                uploadDocumentsDto.IsPortalCall = fileScanningResultDto.metadata.IsPortalCall;
                            }

                            if (!string.IsNullOrEmpty(fileScanningResultDto.metadata.FeedbackId))
                            {
                                uploadDocumentsDto.FeedbackId = new Guid(fileScanningResultDto.metadata.FeedbackId);
                                uploadDocumentsDto.FeedbackFileExtension = fileScanningResultDto.metadata.FeedbackFileExtension;
                            }

                            if (!string.IsNullOrEmpty(fileScanningResultDto.metadata.EsmRequestId))
                            {
                                uploadDocumentsDto.EsmRequestId = fileScanningResultDto.metadata.EsmRequestId;
                            }


                            switch (fileResult.Value)
                            {
                                case 0:
                                case 7:
                                case 39:
                                    List<UploadedDocDetails> Files = new List<UploadedDocDetails>();
                                    Files.Add(new UploadedDocDetails { 
                                        DocumentContent = content,
                                        DocumentName = fileName,
                                        DocumentSize = fileScanningResultDto.metadata.DocumentSize,
                                        DocumentExtension = fileScanningResultDto.metadata.DocumentExtension,
                                        FormDataKeyName = fileScanningResultDto.metadata.FormDataKeyName
                                    });

                                    uploadDocumentsDto.Documents = Files;
                                    break;
                                case 8:
                                case 9:
                                case 12:
                                case 13:
                                case 14:
                                case 15:
                                    return uploadDocumentsDto;
                                default:
                                    return uploadDocumentsDto;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteToFile("01GetFileResult.txt", ex.Message + ",,,," + ex.StackTrace);

                    throw new Exception(ex.Message);
                }
            }
            catch (Exception ex)
            {
                WriteToFile("02GetFileResult.txt", ex.Message + ",,,," + ex.StackTrace);

                throw new Exception(ex.Message);
            }

            return uploadDocumentsDto;
        }

        public void WriteToFile(string fileName, string content)
        {
            string directoryPath = AppDomain.CurrentDomain.BaseDirectory;

            string filePath = Path.Combine(directoryPath, fileName);

            try
            {
                using (FileStream fileStream = File.Create(filePath))
                {
                    using (StreamWriter sw = new StreamWriter(fileStream))
                    {
                        sw.Write(content);
                    }
                }
                Console.WriteLine($"created successfuly : {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(ex));
            }
        }
    }
}

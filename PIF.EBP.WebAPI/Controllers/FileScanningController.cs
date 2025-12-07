using Newtonsoft.Json;
using PIF.EBP.Application.FileScanning;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    public class FileScanningController : ApiController
    {
        private readonly IFileScanningService _FileScanService;
        private bool loggingFileScanningCallbackResults;

        public FileScanningController()
        {
            _FileScanService = WindsorContainerProvider.Container.Resolve<IFileScanningService>();
            bool.TryParse(ConfigurationManager.AppSettings["LogFileScanningCallbackResults"], out bool enableFileScanning);
            loggingFileScanningCallbackResults = enableFileScanning;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("callback")]
        public async Task<HttpResponseMessage> GetFileResult()
        {
            //string content = await Request.Content.ReadAsStringAsync();

            byte[] rawBytes;

            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    rawBytes = memoryStream.ToArray();
                }
            }

            string content = (rawBytes != null && rawBytes.Length > 0)
            ? Convert.ToBase64String(rawBytes)
            : string.Empty;

            var requestHeaders = Request.Headers.ToDictionary(h => h.Key, h => String.Join(",", h.Value));

            if (loggingFileScanningCallbackResults)
            {
                _FileScanService.WriteToFile("callback-RequestHeaders.txt", JsonConvert.SerializeObject(Request.Headers.ToDictionary(h => h.Key, h => String.Join(",", h.Value))));
                _FileScanService.WriteToFile("callback-response.txt", content);
            }
            

            if (requestHeaders.Keys.Contains("dataid"))
            {
                var dataId = requestHeaders["dataid"];
                await _FileScanService.GetFileResult(content, dataId);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Received");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("sanitizedcallback")]
        public async Task<HttpResponseMessage> GetSanitizedFileResult()
        {
            //string content = await Request.Content.ReadAsStringAsync();

            byte[] rawBytes;

            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    rawBytes = memoryStream.ToArray();
                }
            }

            string content = (rawBytes != null && rawBytes.Length > 0)
            ? Convert.ToBase64String(rawBytes)
            : string.Empty;

            var requestHeaders = Request.Headers.ToDictionary(h => h.Key, h => String.Join(",", h.Value));

            if (loggingFileScanningCallbackResults)
            {
                _FileScanService.WriteToFile("sanitized-RequestHeaders.txt", JsonConvert.SerializeObject(Request.Headers.ToDictionary(h => h.Key, h => String.Join(",", h.Value))));
                _FileScanService.WriteToFile("sanitized-response.txt", content);
            }

            if (requestHeaders.Keys.Contains("dataid"))
            {
                var dataId = requestHeaders["dataid"];
                await _FileScanService.GetFileResult(content, dataId);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Received");
        }

        [HttpPost]
        [Route("SendFileToScan")]
        public async Task<IHttpActionResult> SendFileToScan()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var fileContents = provider.Contents;

            byte[] fileBytes;
            string fileName = string.Empty;


            using (var memoryStream = new MemoryStream())
            {
                foreach (var content in fileContents)
                {
                    fileName = string.Empty;

                    var contentDisposition = content.Headers.ContentDisposition;
                    if (contentDisposition != null && contentDisposition.FileName != null)
                    {
                        fileName = contentDisposition.FileName.Trim('"');
                    }

                    byte[] bytes = await content.ReadAsByteArrayAsync();

                    await memoryStream.WriteAsync(bytes, 0, bytes.Length);
                }

                fileBytes = memoryStream.ToArray();
            }

            var response = await _FileScanService.AnalyzeFile(fileBytes, fileName);

            return Ok(response);

        }
    }
}

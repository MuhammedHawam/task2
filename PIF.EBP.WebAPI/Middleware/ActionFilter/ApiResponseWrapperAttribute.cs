using PIF.EBP.WebAPI.Response;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Web.Http.Filters;

namespace PIF.EBP.WebAPI.Middleware.ActionFilter
{
    public class ApiResponseWrapperAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            HttpStatusCode StatusCode = HttpStatusCode.OK;

            if (actionExecutedContext.Response != null)
            {
                object content;
                actionExecutedContext.Response.TryGetContentValue(out content);
                StatusCode = actionExecutedContext.Response.StatusCode;

                var wrappedResponse = new ApiResponse(
                    (int)StatusCode,
                    actionExecutedContext.Response.IsSuccessStatusCode ? "Success" : "Error",
                    actionExecutedContext.Response.IsSuccessStatusCode ? content : null,
                    actionExecutedContext.Response.IsSuccessStatusCode ? null : content);

                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(actionExecutedContext.Response.StatusCode, wrappedResponse);
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        private ApiResponseError GetApiResponseError(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<ApiResponseError>(message);
            }
            catch
            {
                return new ApiResponseError { Code = message };
            }
        }
    }
}
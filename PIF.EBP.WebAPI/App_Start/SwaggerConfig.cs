using Swashbuckle.Application;
using System.Web.Http;

namespace PIF.EBP.WebAPI.App_Start
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;
            if (thisAssembly != null)
            {
                GlobalConfiguration.Configuration.EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "PIF.EBP.WebAPI");
                    c.SchemaId(x => x.FullName);
                    c.OperationFilter<AddMultipleFileUploadAndCustomHeader>();


                }).EnableSwaggerUi(c => 
                {
                    c.EnableOAuth2Support("client_id", "realm", "Swagger UI");
                });
            }
        }
    }
}
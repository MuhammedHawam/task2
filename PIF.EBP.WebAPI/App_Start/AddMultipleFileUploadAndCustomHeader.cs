using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace PIF.EBP.WebAPI.App_Start
{
    public class AddMultipleFileUploadAndCustomHeader : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                operation.parameters = new List<Parameter>();

            // Add custom header (for example, API token)
            operation.parameters.Add(new Parameter
            {
                name = "Authorization",
                @in = "header",
                type = "string",
                required = false,
                description = "API Token required to access this API"
            });

            // Add file parameter for file upload
            operation.consumes.Add("multipart/form-data");  // Ensure Swagger recognizes the form data
            operation.parameters.Add(new Parameter
            {
                name = "files",               // The name of the parameter
                @in = "formData",             // Specifies that it's form data
                type = "file",                // Specifies that it's a file
                required = false,              // Make it required
                description = "Upload multiple files",
                @default = null
            });
        }
    }
}
using FluentValidation;
using FluentValidation.AspNetCore;
using GAIA.Core.Interfaces.Chat;
using GAIA.Core.Services.Chat;
using GAIA.Infra.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.EnableAnnotations();
  options.SupportNonNullableReferenceTypes();
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "GAIA API",
    Version = "v1",
    Description = "GAIA backend HTTP API"
  });

  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
  if (System.IO.File.Exists(xmlPath))
  {
    options.IncludeXmlComments(xmlPath);
  }
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<GAIA.Api.Contracts.Validation.CreateAssessmentRequestValidator>();
// Use automatic 400 responses for invalid ModelState via [ApiController]
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
  options.SuppressModelStateInvalidFilter = false;
});
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));

// Use real or mock service based on configuration
if (builder.Configuration.GetValue<bool>("UseMockService"))
{
  builder.Services.AddSingleton<IAiChatService, MockAiChatService>();
}
else
{
  builder.Services.AddHttpClient<IAiChatService, AiChatService>();
}

var app = builder.Build();

// Expose Swagger first to avoid any SPA fallback interfering
if (app.Environment.IsDevelopment())
{
  app.UseSwagger(c =>
  {
    // Serve JSON at a unique path unlikely to be intercepted by SPA middleware
    c.RouteTemplate = "openapi/{documentName}/openapi.json";
  });
  app.UseSwaggerUI(c =>
  {
    // Point UI to the exact JSON URL
    c.SwaggerEndpoint("/openapi/v1/openapi.json", "GAIA API v1");
    c.RoutePrefix = "docs"; // UI available at /docs
  });
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseExceptionHandler();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

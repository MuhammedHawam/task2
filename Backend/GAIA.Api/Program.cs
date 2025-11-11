using FluentValidation;
using FluentValidation.AspNetCore;
using GAIA.Core.Interfaces.Chat;
using GAIA.Core.Services.Chat;
using GAIA.Infra.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.EnableAnnotations();
  options.SupportNonNullableReferenceTypes();
  options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
  {
    Title = "GAIA API",
    Version = "v1",
    Description = "GAIA backend HTTP API"
  });

  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  if (File.Exists(xmlPath))
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



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseExceptionHandler();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

using FluentValidation;
using FluentValidation.AspNetCore;
using GAIA.Api.Contracts.Assessment.Validation;
using GAIA.Core.Interfaces.Chat;
using GAIA.Core.Services.Chat;
using GAIA.Domain.Framework;
using GAIA.Infra.Configurations;
using GAIA.Infra.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend", policy =>
  {
    policy
      .WithOrigins("http://localhost:52983")
      .AllowAnyMethod()
      .AllowAnyHeader();
  });
});

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
    Description = "GAIA backend HTTP API",
  });

  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  if (File.Exists(xmlPath))
  {
    options.IncludeXmlComments(xmlPath);
  }
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAssessmentDetailsRequestValidator>();
// Use automatic 400 responses for invalid ModelState via [ApiController]
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = false; });

builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddInfraEfCore(builder.Configuration.GetRequiredSection("DbConnections:EfCoreConnection"));

builder.Services.LoadFrameworkModule();

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

await app.Services.MigrateDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseExceptionHandler();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

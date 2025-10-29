using FluentValidation.AspNetCore;
using GAIA.Core.Interfaces.Chat;
using GAIA.Core.Services.Chat;
using GAIA.Infra.Configurations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationAutoValidation();

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

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

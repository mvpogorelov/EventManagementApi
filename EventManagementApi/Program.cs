using EventManagement.Application;
using EventManagement.Infrastructure;
using EventManagement.Infrastructure.Security;
using EventManagement.Presentation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
builder.Services.AddInfrastructure(configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddApplication();
builder.Services.AddPresentation();

var app = builder.Build();

app.ApplayMigrations();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

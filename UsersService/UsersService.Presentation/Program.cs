using UsersService.Application;
using UsersService.Infrastructure;
using UsersService.Infrastructure.Security;
using UsersService.Presentation;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
builder.Services.AddInfrastructure(configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddApplication();
builder.Services.AddPresentation();

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();
app.UseAuthentication();
app.UseAuthorization();
app.ApplyMigrations();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

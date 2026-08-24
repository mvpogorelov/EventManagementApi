using EventsService.Application;
using EventsService.Infrastructure;
using EventsService.Infrastructure.Security;
using EventsService.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT конфигурация не найдена или некорректна");

builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

builder.Services
.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

        ValidateLifetime = true,

        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddInfrastructure(configuration);
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

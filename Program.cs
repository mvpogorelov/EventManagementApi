using EventManagmentApi.Application;
using EventManagmentApi.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPresentation();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

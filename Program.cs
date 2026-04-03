using EventManagmentApi.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();

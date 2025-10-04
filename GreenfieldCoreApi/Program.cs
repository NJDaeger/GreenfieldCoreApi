using GreenfieldCoreApi;
using GreenfieldCoreServices.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices();
builder.Services.ConfigureCommandServices();
builder.Services.ConfigureWebServices();

var app = builder.Build();

app.ConfigureWebApplication();

app.Run();
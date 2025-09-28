using GreenfieldCoreApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices();
builder.Services.ConfigureWebServices();

var app = builder.Build();

app.ConfigureWebApplication();

app.Run();
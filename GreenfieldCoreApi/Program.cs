using GreenfieldCoreApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices();
builder.Configuration.ConfigureConfiguration(builder.Environment);
builder.Services.ConfigureDatabases();
builder.Services.ConfigureWebServices();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureCommandServices();

var app = builder.Build();

await app.Services.PerformDatabaseMigrations();

app.ConfigureWebApplication();

app.Run();

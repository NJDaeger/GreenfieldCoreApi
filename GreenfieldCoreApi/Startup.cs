using System.Diagnostics;
using System.Net;
using Asp.Versioning;
using GreenfieldCoreApi.Transformers;
using GreenfieldCoreDataAccess.Database.Repositories;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.ScriptManager;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Commands;
using GreenfieldCoreServices.Models.BuildApps;
using GreenfieldCoreServices.Models.BuildCodes;
using GreenfieldCoreServices.Models.Clients;
using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Models.Connections.Patreon;
using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Models.Patreon;
using GreenfieldCoreServices.Models.Resources;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services;
using GreenfieldCoreServices.Services.Caching;
using GreenfieldCoreServices.Services.External;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using GreenfieldCoreServices.Services.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GreenfieldCoreApi;

public static class Startup
{
    
    internal static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        
        builder.Host.UseSerilog(Log.Logger);
    }
    
    internal static async Task PerformDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scriptManager = scope.ServiceProvider.GetRequiredService<IScriptManager>();
        await scriptManager.ApplyPendingScripts(CancellationToken.None);
    }
    
    internal static void ConfigureDatabases(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionScope, TransactionScope>();
        services.AddTransient<IScriptManager, ScriptManager>();
        services.AddTransient<IClientRepository, ClientRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IPatreonConnectionRepository, PatreonConnectionRepository>();
        services.AddTransient<IDiscordConnectionRepository, DiscordConnectionRepository>();
        services.AddTransient<ICodeRepository, CodeRepository>();
        services.AddTransient<IApplicationRepository, ApplicationRepository>();
        services.AddTransient<IRedblockRepository, RedblockRepository>();
    }
    
    internal static void ConfigureScheduledTasks(this IServiceCollection services)
    {
        services.AddSingleton<PatreonTokenRefreshTask>();
        services.AddHostedService(sp => sp.GetRequiredService<PatreonTokenRefreshTask>());
        services.AddKeyedSingleton<ITokenRefreshTrigger>("patreon", (sp, _) => sp.GetRequiredService<PatreonTokenRefreshTask>());

        services.AddSingleton<DiscordTokenRefreshTask>();
        services.AddHostedService(sp => sp.GetRequiredService<DiscordTokenRefreshTask>());
        services.AddKeyedSingleton<ITokenRefreshTrigger>("discord", (sp, _) => sp.GetRequiredService<DiscordTokenRefreshTask>());
    }
    
    internal static void ConfigureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IPatreonService, PatreonService>();
        services.AddTransient<IDiscordService, DiscordService>();
        services.AddTransient<IClientAuthService, ClientAuthService>();
        services.AddTransient<ICodeService, CodeService>();
        services.AddTransient<IBuilderApplicationService, BuilderApplicationService>();
        services.AddHttpClient<IPatreonApi, PatreonApi>(client => { client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/"); });
        services.AddHttpClient<IDiscordApi, DiscordApi>(client => { client.BaseAddress = new Uri("https://discord.com"); });
        services.AddHttpClient<IGitHubApi, GitHubApi>(client => { client.BaseAddress = new Uri("https://api.github.com"); });

        services.AddTransient<IResourcePackService, ResourcePackService>();
        services.AddSingleton<TaskStartSignalService>();
    }

    internal static void ConfigureCaching(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService<Guid, Client>, ClientCacheService>();
        services.AddSingleton<ICacheService<long, BuildCode>, BuildCodeCacheService>();
        services.AddSingleton<ICacheService<long, User>, UserCacheService>();
        services.AddSingleton<ICacheService<long, PatreonConnection>, PatreonConnectionCacheService>();
        services.AddSingleton<ICacheService<(long, long), UserPatreonConnection>, UserPatreonConnectionCacheService>();
        services.AddSingleton<ICacheService<long, DiscordConnection>, DiscordConnectionCacheService>();
        services.AddSingleton<ICacheService<(long, long), UserDiscordConnection>, UserDiscordConnectionCacheService>();
        services.AddSingleton<ICacheService<long, BuilderApplication>, BuildAppCacheService>();
        services.AddSingleton<ICacheService<long, PatreonConnectionState>, PatreonConnectionStateCache>();
        services.AddSingleton<ICacheService<(long userId, long patreonConnectionId), PatreonDisconnectState>, PatreonDisconnectStateCache>();
        services.AddSingleton<ICacheService<long, DiscordConnectionState>, DiscordConnectionStateCache>();
        services.AddSingleton<ICacheService<(long userId, long discordConnectionId), DiscordDisconnectState>, DiscordDisconnectStateCache>();
        services.AddSingleton<ICacheService<string, ResourcePackCacheEntry>, ResourcePackCacheService>();
        services.AddSingleton<ICacheService<Guid, DownloadToken>, DownloadTokenCacheService>();
    }

    internal static void ConfigureConfiguration(this IConfigurationBuilder configBuilder, IWebHostEnvironment env)
    {
        configBuilder.SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"connectionstrings.{env.EnvironmentName}.json", optional: false)
            .AddJsonFile($"jwtsettings.{env.EnvironmentName}.json", optional: false)
            .AddJsonFile($"services.{env.EnvironmentName}.json", optional: false)
            .AddEnvironmentVariables();
    }
    
    internal static void ConfigureCommandServices(this IServiceCollection services)
    {
        services.AddHostedService<CommandProcessService>();
        services.AddKeyedTransient<ICommand, ClientCommand>("client");
        services.AddKeyedTransient<ICommand, TasksCommand>("tasks");
    }
    
    internal static void ConfigureAuthentication(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidIssuer = configurationManager.GetValue<string>("jwtSettings:issuer"),
                    ValidAudience = configurationManager.GetValue<string>("jwtSettings:audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configurationManager.GetValue<string>("jwtSettings:key")!))
                };
            });
        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.AddMvc();
    }
    
    internal static void ConfigureWebServices(this IServiceCollection services)
    {
        services.AddApiVersioning(x =>
        {
            x.DefaultApiVersion = new ApiVersion(1, 0);
            x.AssumeDefaultVersionWhenUnspecified = true;
            x.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        services.AddOpenApi(opt =>
        {
            opt.AddDocumentTransformer<ClientCredentialsTransformer>();
            opt.AddDocumentTransformer<TitleTransformer>();
        });
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddControllers();
        services.AddOpenApi();
    }

    internal static void ConfigureWebApplication(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        
        var knownProxies = ParseKnownProxies(logger, app.Configuration["known-proxies"]);
        knownProxies.ForEach(forwardedHeadersOptions.KnownProxies.Add);
        logger.LogInformation("Configured {KnownProxiesCount} known proxies: {Join}", knownProxies.Count, string.Join(", ", knownProxies));
        
        app.UseForwardedHeaders(forwardedHeadersOptions);
        
        app.MapOpenApi();
        app.MapScalarApiReference(options => options
            .WithLayout(ScalarLayout.Classic)
            .WithTitle("Greenfield Core API (" + app.Environment.EnvironmentName + ")")
            .AddPreferredSecuritySchemes("OAuth2")
            .AddClientCredentialsFlow("OAuth2", flow =>
                {
                    flow.TokenUrl = "/api/v1.0/login/token";
                    flow.CredentialsLocation = CredentialsLocation.Body;
                })
            .WithPersistentAuthentication()
        );
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var problemFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var globalLogger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                
                var problemDetails = problemFactory.CreateProblemDetails(
                    context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "An internal server error has occurred!",
                    detail: "A technical error occurred in the server. Please contact support with the trace ID.",
                    instance: exceptionHandlerPathFeature?.Path
                );
                problemDetails.Extensions["traceId"] = traceId;
                globalLogger.LogError("An unhandled exception occurred. Trace ID: {TraceId}", traceId);

                context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
        
        app.MapControllers();
    }
    
    private static List<IPAddress> ParseKnownProxies(ILogger logger, string? proxyString)
    {
        if (string.IsNullOrEmpty(proxyString)) return [];
        return proxyString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => 
                        {
                            if (IPAddress.TryParse(p, out var ipAddress)) return ipAddress;
                            logger.LogWarning("Invalid IP address format for known proxy: {Proxy}", p);
                            return null;
                        })
                        .Where(ip => ip is not null)
                        .ToList()!;
            
    }
    
}
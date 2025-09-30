using System.Net;
using Asp.Versioning;
using GreenfieldCoreServices.Services;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

namespace GreenfieldCoreApi;

public static class Startup
{
    internal static void ConfigureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IUserService, UserService>();
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
        
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddAuthentication();
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
        
        var knownProxies = ParseKnownProxies(logger, app.Configuration["Known-Proxies"]);
        knownProxies.ForEach(forwardedHeadersOptions.KnownProxies.Add);
        logger.LogInformation("Configured {KnownProxiesCount} known proxies: {Join}", knownProxies.Count, string.Join(", ", knownProxies));
        
        app.UseForwardedHeaders(forwardedHeadersOptions);
        
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseAuthentication();
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
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
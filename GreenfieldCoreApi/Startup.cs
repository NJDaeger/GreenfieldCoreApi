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
        var knownProxies = new List<IPAddress>();
        
        // Parse known proxies from command line argument as semicolon-separated string
        var knownProxyString = app.Configuration["known-proxy"];
        if (!string.IsNullOrEmpty(knownProxyString))
        {
            var proxyValues = knownProxyString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var proxyValue in proxyValues)
            {
                var trimmedValue = proxyValue.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedValue))
                {
                    if (IPAddress.TryParse(trimmedValue, out var ipAddress))
                    {
                        knownProxies.Add(ipAddress);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Invalid IP address format for known proxy: {trimmedValue}");
                    }
                }
            }
        }
        
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        
        // Add known proxies to the options
        foreach (var proxy in knownProxies)
        {
            forwardedHeadersOptions.KnownProxies.Add(proxy);
        }
        
        // Log the known proxies being used
        if (knownProxies.Any())
        {
            Console.WriteLine($"Configured {knownProxies.Count} known proxies: {string.Join(", ", knownProxies)}");
        }
        
        app.UseForwardedHeaders(forwardedHeadersOptions);
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        app.UseAuthentication();
        
        app.UseHttpsRedirection();
        app.MapControllers();
    }
    
}
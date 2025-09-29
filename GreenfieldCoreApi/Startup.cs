using System.Runtime.InteropServices;
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
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        app.UseAuthentication();
        
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        app.UseHttpsRedirection();
        app.MapControllers();
    }
    
}
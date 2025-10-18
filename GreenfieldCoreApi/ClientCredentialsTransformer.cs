using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace GreenfieldCoreApi;

public class ClientCredentialsTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        // If any authentication scheme exists that suggests token-based auth, add OAuth2 client credentials
        if (authenticationSchemes.Any())
        {
            document.Components ??= new OpenApiComponents();

            var securitySchemeId = "OAuth2";

            var oauth2Scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    ClientCredentials = new OpenApiOAuthFlow
                    {
                        // relative token URL used by the API
                        TokenUrl = new Uri("/api/v1.0/login/login", UriKind.Relative)
                    }
                },
                Description = "OAuth2 Client Credentials"
            };

            document.Components.SecuritySchemes[securitySchemeId] = oauth2Scheme;

            // Add the OAuth2 scheme as a requirement for the API as a whole (no scopes)
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = securitySchemeId, Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
            });
        }
    }
}
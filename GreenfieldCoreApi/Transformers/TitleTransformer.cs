using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace GreenfieldCoreApi.Transformers;

public class TitleTransformer(IWebHostEnvironment env) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, 
        CancellationToken cancellationToken)
    {
        var baseTitle = $"({env.EnvironmentName}) {document.Info.Title}";
        document.Info.Title = baseTitle;
        return Task.CompletedTask;
    }
}
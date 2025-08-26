using System.Diagnostics;

namespace SettlersOfCrutan.AppHost;
internal static class ResourceBuilderExtensions
{
    internal static IResourceBuilder<T> WithScalar<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs("scalar-docs", "Scalar Documentation", "scalar/v1");
    }
    private static IResourceBuilder<T> WithOpenApiDocs<T>(this IResourceBuilder<T> builder,
        string name,
        string displayName,
        string openApiUiPath)
        where T : IResourceWithEndpoints
    {
        return builder.WithCommand(
            name,
            displayName,
            executeCommand: _ =>
            {
                try
                {
                    var endpoint = builder.GetEndpoint("https");

                    var url = $"{endpoint.Url}/{openApiUiPath}";

                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

                    return Task.FromResult(new ExecuteCommandResult { Success = true });
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new ExecuteCommandResult { Success = false, ErrorMessage = ex.ToString() });
                }
            },
            new CommandOptions()
            {
                IconName = "Document",
                IconVariant = IconVariant.Filled
            });
    }
}

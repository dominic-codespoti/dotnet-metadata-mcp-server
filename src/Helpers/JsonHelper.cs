using System.Text.Json;
using DotNetMetadataMcpServer.Configuration;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };

    public static string Serialize(this ToolsConfiguration configuration, object value)
    {
        return configuration.IntendResponse
            ? JsonSerializer.Serialize(value, DefaultOptions)
            : JsonSerializer.Serialize(value);
    }
}
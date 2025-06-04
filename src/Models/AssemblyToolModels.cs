using System.Text.Json.Serialization;
using DotNetMetadataMcpServer.Models.Base;

namespace DotNetMetadataMcpServer.Models
{
    public class AssemblyToolParameters : PagedRequestWithFilter
    {
        public required string ProjectFileAbsolutePath { get; init; }

        public override string ToString()
        {
            return $"ProjectFileAbsolutePath: {ProjectFileAbsolutePath}, " +
                   $"FullTextFiltersWithWildCardSupport: [{string.Join(", ", FullTextFiltersWithWildCardSupport)}]";
        }
    }
    
    [JsonSerializable(typeof(AssemblyToolParameters))]
    public partial class AssemblyToolParametersJsonContext : JsonSerializerContext
    {
    }

    public class AssemblyToolResponse : PagedResponse
    {
        public IEnumerable<string> AssemblyNames { get; set; } = new List<string>();
    }
}
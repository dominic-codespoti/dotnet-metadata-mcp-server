using System.ComponentModel;
using DotNetMetadataMcpServer.Helpers;
using DotNetMetadataMcpServer.Models;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.Services
{
    [McpServerToolType]
    public class AssemblyToolService
    {
        private readonly IDependenciesScanner _scanner;
        private readonly ILogger<AssemblyToolService> _logger;

        public AssemblyToolService(IDependenciesScanner scanner, ILogger<AssemblyToolService> logger)
        {
            _scanner = scanner;
            _logger = logger;
        }

        [McpServerTool(Name = "GetReferencedAssemblies")] 
        [Description("Retrieves referenced assemblies based on filters and pagination (doesn't extract data from referenced projects. " +
                     "Notice that the project must be built before scanning.")]
        public AssemblyToolResponse GetAssemblies(
            [Description("TODO")] string projectFileAbsolutePath, 
            [Description("TODO")] List<string> filters, 
            [Description("TODO")] int pageNumber, 
            [Description("TODO")] int pageSize)
        {
            using var _ = _logger.BeginScope("{AssemblyToolExecutionUid}", Guid.NewGuid());   
            _logger.LogInformation("Received request to retrieve assemblies list with: {@Parameters}", new
            {
                ProjectFileAbsolutePath = projectFileAbsolutePath,
                Filters = filters,
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            try
            {
                var metadata = _scanner.ScanProject(projectFileAbsolutePath);
                // Get main assembly name and dependency names from full data
                var assemblies = new List<string> { Path.GetFileNameWithoutExtension(metadata.AssemblyPath) };
                assemblies.AddRange(metadata.Dependencies.Select(d => d.Name));

                if (filters.Any())
                {
                    var predicates = filters.Select(FilteringHelper.PrepareFilteringPredicate).ToList();
                    assemblies = assemblies.Where(a => predicates.Any(predicate => predicate.Invoke(a))).ToList();
                }
            
                var (paged, availablePages) = PaginationHelper.FilterAndPaginate(assemblies, _ => true, pageNumber, pageSize);
                return new AssemblyToolResponse
                {
                    AssemblyNames = paged,
                    CurrentPage = pageNumber,
                    AvailablePages = availablePages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning project {Path}", projectFileAbsolutePath);
                throw; // MCP will generate ErrorResponse itself
            }
        }
    }
}
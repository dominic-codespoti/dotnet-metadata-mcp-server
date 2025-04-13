using System.ComponentModel;
using DotNetMetadataMcpServer.Helpers;
using DotNetMetadataMcpServer.Models;
using ModelContextProtocol.Server;

namespace DotNetMetadataMcpServer.Services
{
    [McpServerToolType]
    public class TypeToolService
    {
        private readonly IDependenciesScanner _scanner;
        private readonly ILogger<TypeToolService> _logger;

        public TypeToolService(IDependenciesScanner scanner, ILogger<TypeToolService> logger)
        {
            _scanner = scanner;
            _logger = logger;
        }

        [McpServerTool(Name = "NamespaceTypesExplorer")] 
        [Description("Retrieves types from specified namespaces supporting filters and pagination." +
                     "Notice that the project must be built before scanning.")]
        public TypeToolResponse GetTypes(
            [Description("TODO")] string projectFileAbsolutePath, 
            [Description("TODO")] List<string> allowedNamespaces, 
            [Description("TODO")] List<string> filters, 
            [Description("TODO")] int pageNumber, 
            [Description("TODO")] int pageSize)
        {
            using var _ = _logger.BeginScope("{TypeToolExecutionUid}", Guid.NewGuid());
            _logger.LogInformation("Received request to retrieve types with {@Parameters}", new
            {
                ProjectFileAbsolutePath = projectFileAbsolutePath,
                AllowedNamespaces = allowedNamespaces,
                Filters = filters,
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            try
            {
                var metadata = _scanner.ScanProject(projectFileAbsolutePath);
                // Collect all types from project and dependencies.
                var allTypes = metadata.ProjectTypes.Concat(metadata.Dependencies.SelectMany(d => d.Types));
            
                // If allowed namespaces are provided, only retain types whose namespace (the part before the last '.') is allowed.
                if (allowedNamespaces.Any())
                {
                    allTypes = allTypes.Where(t => 
                        !string.IsNullOrEmpty(t.FullName) && 
                        t.FullName.Contains('.') && 
                        allowedNamespaces.Contains(t.FullName.Substring(0, t.FullName.LastIndexOf('.')), StringComparer.OrdinalIgnoreCase)
                    );
                }
            
                // Apply additional filter if provided.
                if (filters.Any())
                {
                    var predicates = filters.Select(FilteringHelper.PrepareFilteringPredicate).ToList();
                    allTypes = allTypes.Where(t => predicates.Any(predicate => predicate.Invoke(t.FullName)));
                }
            
                var allTypesList = allTypes.Select(TypeInfoModelMapper.ToSimpleTypeInfo).ToList();
                var (paged, availablePages) = PaginationHelper.FilterAndPaginate(allTypesList, _ => true, pageNumber, pageSize);
            
                return new TypeToolResponse
                {
                    TypeData = paged,
                    CurrentPage = pageNumber,
                    AvailablePages = availablePages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving types");
                throw;
            }
        }
    }
}
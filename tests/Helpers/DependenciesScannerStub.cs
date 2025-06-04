using DotNetMetadataMcpServer;

public class DependenciesScannerStub : IDependenciesScanner
{
    private ProjectMetadata projectMetadata;
    private IEnumerable<string> namespaces;

    public DependenciesScannerStub(ProjectMetadata? projectMetadata = null, IEnumerable<string>? namespaces = null)
    {
        this.projectMetadata = projectMetadata ?? new ProjectMetadata();
        this.namespaces = namespaces ?? [];
    }

    public void Dispose()
    {
    }

    public ProjectMetadata ScanProject(string projectFileAbsolutePath)
    {
        return projectMetadata;
    }

    public IEnumerable<string> GetAllNamespaces(string projectFileAbsolutePath)
    {
        return namespaces;
    }
}
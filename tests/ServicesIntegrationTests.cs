using DotNetMetadataMcpServer;
using DotNetMetadataMcpServer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using MetadataExplorerTest.Helpers;

namespace MetadataExplorerTest;

[TestFixture]
public class ServicesIntegrationTests
{
    private string _testProjectPath;
    private DependenciesScannerStub _scanner;
    private static List<string> _foundAssemblies = null!;
    private static List<string> _foundNamespaces = null!;
    private Mock<ILogger<NuGetToolService>> _nugetLoggerMock;

    [SetUp]
    public void Setup()
    {
        _testProjectPath = "dummy/path/to/project.csproj";
        _scanner = new DependenciesScannerStub();
        _nugetLoggerMock = new Mock<ILogger<NuGetToolService>>();
    }
    
    [TearDown]
    public void TearDown()
    {
        _scanner.Dispose();
    }
    
    [Test, Order(1)]
    public void AssemblyToolService_ShouldReturnAssemblies()
    {
        var service = new AssemblyToolService(_scanner);
        var response = service.GetAssemblies(_testProjectPath, new List<string>(), 1, 50);

        Assert.That(response.AssemblyNames, Is.Not.Null);
        Assert.That(response.AssemblyNames, Is.TypeOf<List<string>>());

        _foundAssemblies = response.AssemblyNames.Take(2).ToList();
    }

    [Test, Order(2)]
    public void NamespaceToolService_ShouldReturnNamespacesFromFoundAssemblies()
    {
        Assert.That(_foundAssemblies, Is.Not.Null, "Previous test must be run first");

        var service = new NamespaceToolService(_scanner);
        var response = service.GetNamespaces(_testProjectPath, _foundAssemblies, new List<string>(), 1, 50);

        Assert.That(response.Namespaces, Is.Not.Null);
        Assert.That(response.Namespaces, Is.TypeOf<List<string>>());

        _foundNamespaces = response.Namespaces.Take(1).Concat(response.Namespaces.TakeLast(1)).ToList();
    }

    [Test, Order(3)]
    public void TypeToolService_ShouldReturnTypesFromFoundNamespaces()
    {
        Assert.That(_foundNamespaces, Is.Not.Null, "Previous tests must be run first");

        var service = new TypeToolService(_scanner);
        var response = service.GetTypes(_testProjectPath, _foundNamespaces, new List<string>(), 1, 50);

        Assert.That(response.TypeData, Is.Not.Null);
        Assert.That(response.TypeData, Is.TypeOf<List<DotNetMetadataMcpServer.Models.SimpleTypeInfo>>());
    }
    
    [Test, Order(4)]
    public async Task NuGetToolService_SearchPackages_ShouldReturnResults()
    {
        var service = new NuGetToolService(_nugetLoggerMock.Object);
        var response = await service.SearchPackagesAsync("Newtonsoft.Json", new List<string>(), false, 1, 10);

        Assert.That(response.Packages, Is.Not.Null);
        Assert.That(response.Packages, Is.TypeOf<List<DotNetMetadataMcpServer.Models.NuGetPackageInfo>>());
    }
    
    [Test, Order(5)]
    public async Task NuGetToolService_GetPackageVersions_ShouldReturnVersionsWithDependencies()
    {
        var service = new NuGetToolService(_nugetLoggerMock.Object);
        var response = await service.GetPackageVersionsAsync("Newtonsoft.Json", new List<string>(), false, 1, 10);

        Assert.That(response.Versions, Is.Not.Null);
        Assert.That(response.Versions, Is.TypeOf<List<DotNetMetadataMcpServer.Models.NuGetPackageInfo>>());
        Assert.That(response.PackageId, Is.EqualTo("Newtonsoft.Json"));
    }
}


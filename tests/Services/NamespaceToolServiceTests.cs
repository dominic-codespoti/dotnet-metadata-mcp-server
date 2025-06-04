using DotNetMetadataMcpServer;
using DotNetMetadataMcpServer.Services;
using MetadataExplorerTest.Helpers;

namespace MetadataExplorerTest.Services;

[TestFixture]
public class NamespaceToolServiceTests
{
    private string _testProjectPath;
    private DependenciesScannerStub _scanner;
    private NamespaceToolService _service;
    private const string DummyAssembly = "TestAssembly";

    [SetUp]
    public void Setup()
    {
        _testProjectPath = "dummy/path/to/project.csproj";
        _scanner = new DependenciesScannerStub();
        _service = new NamespaceToolService(_scanner);
    }

    [TearDown]
    public void TearDown()
    {
        _scanner.Dispose();
    }

    [Test]
    public void GetNamespaces_WithAllowedAssemblies_ReturnsFilteredResults()
    {
        var allowedAssemblies = new List<string> { DummyAssembly };
        var response = _service.GetNamespaces(_testProjectPath, allowedAssemblies, new List<string>(), 1, 20);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Namespaces, Is.TypeOf<List<string>>());
    }

    [Test]
    public void GetNamespaces_WithFilters_ReturnsFilteredResults()
    {
        var filters = new List<string> { "*Helpers" };
        var response = _service.GetNamespaces(_testProjectPath, new List<string>(), filters, 1, 20);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Namespaces, Is.TypeOf<List<string>>());
    }

    [Test]
    public void GetNamespaces_Pagination_ReturnsCorrectPage()
    {
        const int pageSize = 5;
        var response1 = _service.GetNamespaces(_testProjectPath, new List<string>(), new List<string>(), 1, pageSize);
        var response2 = _service.GetNamespaces(_testProjectPath, new List<string>(), new List<string>(), 2, pageSize);
        Assert.That(response1, Is.Not.Null);
        Assert.That(response2, Is.Not.Null);
        Assert.That(response1.CurrentPage, Is.EqualTo(1));
        Assert.That(response2.CurrentPage, Is.EqualTo(2));
    }

    [Test]
    public void GetNamespaces_WithInvalidPageNumber_ReturnsEmptyResults()
    {
        const int invalidPageNumber = 10000;
        var response = _service.GetNamespaces(_testProjectPath, new List<string>(), new List<string>(), invalidPageNumber, 20);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Namespaces, Is.TypeOf<List<string>>());
        Assert.That(response.CurrentPage, Is.EqualTo(invalidPageNumber));
    }
}

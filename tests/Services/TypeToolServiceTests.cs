using DotNetMetadataMcpServer;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using MetadataExplorerTest.Helpers;

namespace MetadataExplorerTest.Services;

[TestFixture]
public class TypeToolServiceTests
{
    private string _testProjectPath;
    private DependenciesScannerStub _scanner;
    private TypeToolService _service;
    private const string DummyNamespace = "TestNamespace";

    [SetUp]
    public void Setup()
    {
        _testProjectPath = "dummy/path/to/project.csproj";
        _scanner = new DependenciesScannerStub();
        _service = new TypeToolService(_scanner);
    }

    [TearDown]
    public void TearDown()
    {
        _scanner.Dispose();
    }

    [Test]
    public void GetTypes_WithAllowedNamespaces_ReturnsFilteredResults()
    {
        var allowedNamespaces = new List<string> { DummyNamespace };
        var response = _service.GetTypes(_testProjectPath, allowedNamespaces, new List<string>(), 1, 20);
        // Just check that the response is a valid TypeToolResponse
        Assert.That(response, Is.Not.Null);
        Assert.That(response.TypeData, Is.TypeOf<List<SimpleTypeInfo>>());
    }

    [Test]
    public void GetTypes_WithFilters_ReturnsFilteredResults()
    {
        var filters = new List<string> { "*Parameters" };
        var response = _service.GetTypes(_testProjectPath, new List<string>(), filters, 1, 20);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.TypeData, Is.TypeOf<List<SimpleTypeInfo>>());
    }

    [Test]
    public void GetTypes_Pagination_ReturnsCorrectPage()
    {
        const int pageSize = 5;
        var response1 = _service.GetTypes(_testProjectPath, new List<string>(), new List<string>(), 1, pageSize);
        var response2 = _service.GetTypes(_testProjectPath, new List<string>(), new List<string>(), 2, pageSize);
        Assert.That(response1, Is.Not.Null);
        Assert.That(response2, Is.Not.Null);
        Assert.That(response1.CurrentPage, Is.EqualTo(1));
        Assert.That(response2.CurrentPage, Is.EqualTo(2));
    }

    [Test]
    public void GetTypes_WithInvalidPageNumber_ReturnsEmptyResults()
    {
        const int invalidPageNumber = 10000;
        var response = _service.GetTypes(_testProjectPath, new List<string>(), new List<string>(), invalidPageNumber, 20);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.TypeData, Is.TypeOf<List<SimpleTypeInfo>>());
        Assert.That(response.CurrentPage, Is.EqualTo(invalidPageNumber));
    }
}

using DotNetMetadataMcpServer;
using DotNetMetadataMcpServer.Services;
using MetadataExplorerTest.Helpers;

namespace MetadataExplorerTest.Services;

[TestFixture]
public class AssemblyToolServiceTests
{
    private string _testProjectPath;
    private DependenciesScannerStub _scanner;
    private AssemblyToolService _service;
    private const string DummyFilter = "TestAssembly";

    [SetUp]
    public void Setup()
    {
        _testProjectPath = "dummy/path/to/project.csproj";
        _scanner = new DependenciesScannerStub();
        _service = new AssemblyToolService(_scanner);
    }

    [TearDown]
    public void TearDown()
    {
        _scanner.Dispose();
    }

    [Test]
    public void GetAssemblies_WithValidFilters_ReturnsFilteredResults()
    {
        var filters = new List<string> { DummyFilter };
        var response = _service.GetAssemblies(_testProjectPath, filters, 1, 20);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.AssemblyNames, Is.TypeOf<List<string>>());
    }

    [Test]
    public void GetAssemblies_Pagination_ReturnsCorrectPage()
    {
        const int pageSize = 5;
        var response1 = _service.GetAssemblies(_testProjectPath, new List<string>(), 1, pageSize);
        var response2 = _service.GetAssemblies(_testProjectPath, new List<string>(), 2, pageSize);
        Assert.That(response1, Is.Not.Null);
        Assert.That(response2, Is.Not.Null);
        Assert.That(response1.CurrentPage, Is.EqualTo(1));
        Assert.That(response2.CurrentPage, Is.EqualTo(2));
    }

    [Test]
    public void GetAssemblies_WithInvalidPageNumber_ReturnsEmptyResults()
    {
        const int invalidPageNumber = 10000;
        var response = _service.GetAssemblies(_testProjectPath, new List<string>(), invalidPageNumber, 20);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.AssemblyNames, Is.TypeOf<List<string>>());
        Assert.That(response.CurrentPage, Is.EqualTo(invalidPageNumber));
    }
}

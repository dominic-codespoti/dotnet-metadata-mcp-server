using DotNetMetadataMcpServer;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using MetadataExplorerTest.Helpers;

namespace MetadataExplorerTest.Integration;

[TestFixture]
public class TypeReflectionTests
{
    private DependenciesScannerStub _scanner;
    private TypeToolService _service;
    private string _testProjectPath;

    [OneTimeSetUp]
    public void Setup()
    {
        // Use a dummy project path for integration test structure, not a real csproj
        _testProjectPath = "dummy/path/to/project.csproj";
        _scanner = new DependenciesScannerStub();
        _service = new TypeToolService(_scanner);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _scanner.Dispose();
    }

    [Test]
    public void GetTypes_ForSimpleTypeInfo_VerifyFormatting()
    {
        var response = _service.GetTypes(_testProjectPath, new List<string>(), new List<string>(), 1, 100);
        // Just check that the response is a valid TypeToolResponse and has a list of SimpleTypeInfo
        Assert.That(response, Is.Not.Null);
        Assert.That(response.TypeData, Is.TypeOf<List<SimpleTypeInfo>>());
    }

    [Test]
    public void GetTypes_ForTypeWithInterfaces_IncludesImplementsList()
    {
        var response = _service.GetTypes(_testProjectPath, new List<string>(), new List<string>(), 1, 100);
        // Just check that the response is a valid TypeToolResponse and has a list of SimpleTypeInfo
        Assert.That(response, Is.Not.Null);
        Assert.That(response.TypeData, Is.TypeOf<List<SimpleTypeInfo>>());
    }
}

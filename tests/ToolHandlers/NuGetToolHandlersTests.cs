using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Models;
using DotNetMetadataMcpServer.Services;
using DotNetMetadataMcpServer.ToolHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace MetadataExplorerTest.ToolHandlers;

[TestFixture]
public class NuGetToolHandlersTests
{
    private Mock<IOptions<ToolsConfiguration>> _toolsConfigurationMock;
    private NuGetToolService _nuGetToolService;
    private Mock<ILoggerFactory> _loggerFactoryMock;
    private ToolsConfiguration _toolsConfiguration;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        _toolsConfiguration = new ToolsConfiguration
        {
            DefaultPageSize = 10,
            IntendResponse = true
        };
        _toolsConfigurationMock = new Mock<IOptions<ToolsConfiguration>>();
        _toolsConfigurationMock.Setup(x => x.Value).Returns(_toolsConfiguration);
        _nuGetToolService = new NuGetToolService(Mock.Of<ILogger<NuGetToolService>>());
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        // Setup DI
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<ToolsConfiguration>>(_toolsConfigurationMock.Object);
        services.AddSingleton<NuGetToolService>(_nuGetToolService);
        services.AddSingleton<ILoggerFactory>(_loggerFactoryMock.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [Test]
    public async Task NuGetPackageSearchHandleAsync_ReturnsExpectedResult()
    {
        // Arrange
        var parameters = new NuGetPackageSearchParameters
        {
            SearchQuery = "Newtonsoft.Json",
            PageNumber = 1,
            FullTextFiltersWithWildCardSupport = new List<string>()
        };
        // Act
        var result = await NuGetToolHandlers.NuGetPackageSearchHandleAsync(
            parameters,
            _serviceProvider);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("Newtonsoft.Json"));
    }

    [Test]
    public async Task NuGetPackageVersionsHandleAsync_ReturnsExpectedResult()
    {
        // Arrange
        var parameters = new NuGetPackageVersionParameters
        {
            PackageId = "Newtonsoft.Json",
            PageNumber = 1,
            FullTextFiltersWithWildCardSupport = new List<string>()
        };
        // Act
        var result = await NuGetToolHandlers.NuGetPackageVersionsHandleAsync(
            parameters,
            _serviceProvider);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("Newtonsoft.Json"));
        Assert.That(result, Does.Contain("DependencyGroups"));
    }
}
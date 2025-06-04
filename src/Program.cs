using DotNetMetadataMcpServer;
using DotNetMetadataMcpServer.Configuration;
using DotNetMetadataMcpServer.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var builder = Host.CreateApplicationBuilder();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
    loggingBuilder.AddConsole();
});

builder.Services.Configure<ToolsConfiguration>(configuration.GetSection(ToolsConfiguration.SectionName));

builder.Services.AddSingleton<MsBuildHelper>();
builder.Services.AddSingleton<ReflectionTypesCollector>();
builder.Services.AddSingleton(typeof(IDependenciesScanner), typeof(DependenciesScanner));

builder.Services.AddSingleton<AssemblyToolService>();
builder.Services.AddSingleton<NamespaceToolService>();
builder.Services.AddSingleton<TypeToolService>();
builder.Services.AddSingleton<NuGetToolService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

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

builder.Services.AddScoped<MsBuildHelper>();
builder.Services.AddScoped<ReflectionTypesCollector>();
builder.Services.AddScoped(typeof(IDependenciesScanner), typeof(DependenciesScanner));

builder.Services.AddScoped<AssemblyToolService>();
builder.Services.AddScoped<NamespaceToolService>();
builder.Services.AddScoped<TypeToolService>();
builder.Services.AddScoped<NuGetToolService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

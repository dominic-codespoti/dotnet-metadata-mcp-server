using ModelContextProtocol.Server;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace DotNetMetadataMcpServer.ConfigurationExtensions;

/// <summary>
/// Replacement methods for <see cref="McpServerBuilderExtensions"/> with scoped tools instead of singleton.
/// </summary>
public static class McpServerBuilderCustomExtensions
{
    #region WithTools
    private const string WithToolsRequiresUnreferencedCodeMessage =
        $"The non-generic {nameof(WithScopedTools)} and {nameof(WithScopedToolsFromAssembly)} methods require dynamic lookup of method metadata" +
        $"and may not work in Native AOT. Use the generic {nameof(WithScopedTools)} method instead.";

    /// <summary>Adds <see cref="McpServerTool"/> instances to the service collection backing <paramref name="builder"/>.</summary>
    /// <typeparam name="TToolType">The tool type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="serializerOptions">The serializer options governing tool parameter marshalling.</param>
    /// <returns>The builder provided in <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method discovers all instance and static methods (public and non-public) on the specified <typeparamref name="TToolType"/>
    /// type, where the methods are attributed as <see cref="McpServerToolAttribute"/>, and adds an <see cref="McpServerTool"/>
    /// instance for each. For instance methods, an instance will be constructed for each invocation of the tool.
    /// </remarks>
    public static IMcpServerBuilder WithScopedTools<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors)] TToolType>(
        this IMcpServerBuilder builder,
        JsonSerializerOptions? serializerOptions = null)
    {
        Throw.IfNull(builder);

        foreach (var toolMethod in typeof(TToolType).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (toolMethod.GetCustomAttribute<McpServerToolAttribute>() is not null)
            {
                builder.Services.AddScoped((Func<IServiceProvider, McpServerTool>)(toolMethod.IsStatic ?
                    services => McpServerTool.Create(toolMethod, options: new() { Services = services, SerializerOptions = serializerOptions }) :
                    services => McpServerTool.Create(toolMethod, typeof(TToolType), new() { Services = services, SerializerOptions = serializerOptions })));
            }
        }

        return builder;
    }

    /// <summary>Adds <see cref="McpServerTool"/> instances to the service collection backing <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="toolTypes">Types with marked methods to add as tools to the server.</param>
    /// <param name="serializerOptions">The serializer options governing tool parameter marshalling.</param>
    /// <returns>The builder provided in <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="toolTypes"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method discovers all instance and static methods (public and non-public) on the specified <paramref name="toolTypes"/>
    /// types, where the methods are attributed as <see cref="McpServerToolAttribute"/>, and adds an <see cref="McpServerTool"/>
    /// instance for each. For instance methods, an instance will be constructed for each invocation of the tool.
    /// </remarks>
    [RequiresUnreferencedCode(WithToolsRequiresUnreferencedCodeMessage)]
    public static IMcpServerBuilder WithScopedTools(this IMcpServerBuilder builder, IEnumerable<Type> toolTypes, JsonSerializerOptions? serializerOptions = null)
    {
        Throw.IfNull(builder);
        Throw.IfNull(toolTypes);

        foreach (var toolType in toolTypes)
        {
            if (toolType is not null)
            {
                foreach (var toolMethod in toolType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (toolMethod.GetCustomAttribute<McpServerToolAttribute>() is not null)
                    {
                        builder.Services.AddScoped((Func<IServiceProvider, McpServerTool>)(toolMethod.IsStatic ?
                            services => McpServerTool.Create(toolMethod, options: new() { Services = services , SerializerOptions = serializerOptions }) :
                            services => McpServerTool.Create(toolMethod, toolType, new() { Services = services , SerializerOptions = serializerOptions })));
                    }
                }
            }
        }

        return builder;
    }

    /// <summary>
    /// Adds types marked with the <see cref="McpServerToolTypeAttribute"/> attribute from the given assembly as tools to the server.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="serializerOptions">The serializer options governing tool parameter marshalling.</param>
    /// <param name="toolAssembly">The assembly to load the types from. If <see langword="null"/>, the calling assembly will be used.</param>
    /// <returns>The builder provided in <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method scans the specified assembly (or the calling assembly if none is provided) for classes
    /// marked with the <see cref="McpServerToolTypeAttribute"/>. It then discovers all methods within those
    /// classes that are marked with the <see cref="McpServerToolAttribute"/> and registers them as <see cref="McpServerTool"/>s 
    /// in the <paramref name="builder"/>'s <see cref="IServiceCollection"/>.
    /// </para>
    /// <para>
    /// The method automatically handles both static and instance methods. For instance methods, a new instance
    /// of the containing class will be constructed for each invocation of the tool.
    /// </para>
    /// <para>
    /// Tools registered through this method can be discovered by clients using the <c>list_tools</c> request
    /// and invoked using the <c>call_tool</c> request.
    /// </para>
    /// <para>
    /// Note that this method performs reflection at runtime and may not work in Native AOT scenarios. For
    /// Native AOT compatibility, consider using the generic <see cref="WithTools{TToolType}"/> method instead.
    /// </para>
    /// </remarks>
    [RequiresUnreferencedCode(WithToolsRequiresUnreferencedCodeMessage)]
    public static IMcpServerBuilder WithScopedToolsFromAssembly(this IMcpServerBuilder builder, Assembly? toolAssembly = null, JsonSerializerOptions? serializerOptions = null)
    {
        Throw.IfNull(builder);

        toolAssembly ??= Assembly.GetCallingAssembly();

        return builder.WithTools(
            from t in toolAssembly.GetTypes()
            where t.GetCustomAttribute<McpServerToolTypeAttribute>() is not null
            select t,
            serializerOptions);
    }
    #endregion
}
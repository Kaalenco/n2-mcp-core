using McpCore.JsonRpc;
using McpCore.Protocol;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpCore.Server;

/// <summary>
/// Base implementation of MCP server.
/// </summary>
/// <remarks>
/// <para>
/// Initialization follows the two-phase MCP handshake:
/// </para>
/// <list type="number">
///   <item>
///     The client sends an <c>initialize</c> request. The server responds with its capabilities
///     but does <em>not</em> yet mark itself as ready. <see cref="Initialized"/> remains
///     <see langword="false"/>.
///   </item>
///   <item>
///     The client sends a <c>notifications/initialized</c> notification to confirm it has
///     received the capabilities. At this point the server calls <see cref="GetAvailableTools"/>,
///     indexes all tools by name into an internal case-insensitive dictionary, and sets
///     <see cref="Initialized"/> to <see langword="true"/>.
///   </item>
/// </list>
/// <para>
/// Only after phase 2 is complete can <c>tools/list</c> and <c>tools/call</c> be used.
/// Both methods throw <see cref="InvalidOperationException"/> when called before the server
/// is initialized.
/// </para>
/// <para>
/// Subclasses must implement <see cref="GetAvailableTools"/> and assign a
/// <see cref="McpTool.CallAsync"/> delegate to every tool that should be callable.
/// The <c>CallToolAsync</c> dispatch is handled by the base class and cannot be overridden.
/// </para>
/// </remarks>
public abstract class McpServer : IMcpServer
{
    private readonly Dictionary<string, Func<object?, Task<object>>> _methodHandlers = new();
    private readonly McpServerInfo _serverInfo;
    private readonly McpServerCapabilities _capabilities;
    private bool _initialized;

    public bool Initialized => _initialized;

    protected McpServer(McpServerInfo serverInfo, McpServerCapabilities capabilities)
    {
        _serverInfo = serverInfo;
        _capabilities = capabilities;

        RegisterDefaultHandlers();
    }

    public static readonly JsonSerializerOptions Options = new()
    {
        AllowOutOfOrderMetadataProperties = true,
        AllowTrailingCommas = true,
        Converters = {
                new JsonStringEnumConverter(allowIntegerValues: true),
            },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        MaxDepth = 32,
    };

    public virtual async Task<JsonRpcResponse> ProcessRequestAsync(JsonRpcRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            if (!_methodHandlers.TryGetValue(request.Method, out Func<object?, Task<object>>? handler))
            {
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new JsonRpcError
                    {
                        Code = JsonRpcErrorCodes.MethodNotFound,
                        Message = $"Method '{request.Method}' not found"
                    }
                };
            }

            object result = await handler(request.Params);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (ArgumentException ex)
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = JsonRpcErrorCodes.InvalidParams,
                    Message = ex.Message
                }
            };
        }
        catch (Exception ex)
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = JsonRpcErrorCodes.InternalError,
                    Message = ex.Message
                }
            };
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    public virtual Task<McpInitializeResult> InitializeAsync(McpInitializeParams parameters)
    {
        // Don't set _initialized here - wait for the initialized notification
        return Task.FromResult(new McpInitializeResult
        {
            ProtocolVersion = "2024-11-05",
            ServerInfo = _serverInfo,
            Capabilities = _capabilities
        });
    }

    public Task<McpToolsListResult> GetToolsListAsync()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Server not initialized");
        }

        return Task.FromResult(new McpToolsListResult
        {
            Tools = GetAvailableTools()
        });
    }

    public Task<McpToolCallResult> CallToolAsync(McpToolCallParams parameters)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Server not initialized");
        }
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }
        var normalizedToolName = parameters.Name.Trim();
        var tool = McpTools.TryGetValue(normalizedToolName, out McpTool? foundTool) ? foundTool : null;
        if (tool == null)
        {
            throw new ArgumentException($"Tool '{normalizedToolName}' not found");
        }
        if(tool.CallAsync == null)
        {
            throw new InvalidOperationException($"Tool '{normalizedToolName}' is not callable");
        }

        return tool.CallAsync.Invoke(parameters.Arguments);
    }

    private readonly Dictionary<string, McpTool> McpTools = new Dictionary<string, McpTool>(StringComparer.InvariantCultureIgnoreCase);

    protected abstract McpTool[] GetAvailableTools();

    protected void RegisterMethodHandler(string method, Func<object?, Task<object>> handler)
    {
        _methodHandlers[method] = handler;
    }

    private void RegisterDefaultHandlers()
    {
        RegisterMethodHandler(McpMethods.Initialize, async (params_) =>
        {
            McpInitializeParams parameters = DeserializeParams<McpInitializeParams>(params_);
            return await InitializeAsync(parameters);
        });

        RegisterMethodHandler(McpMethods.Initialized, async (_) =>
        {
            // The initialized notification is sent by the client after receiving the initialize response
            // This is part of the MCP handshake protocol

            // get the available tools and put them in a dictionary
            var tools = GetAvailableTools();
            if(tools == null || tools.Length == 0)
            {
                throw new InvalidOperationException("GetAvailableTools returned null or empty array");
            }
            foreach(var tool in tools)
            {
                if(string.IsNullOrWhiteSpace(tool.Name))
                {
                    throw new InvalidOperationException("Tool name cannot be null or whitespace");
                }
                McpTools[tool.Name] = tool;
            }

            // if the class has an initializer, call it now that we know the client is fully initialized and ready to receive notifications

            // Mark the server as fully initialized now
            await Task.Delay(1);

            _initialized = true;
            // We don't need to return anything for notifications, but we need to handle them
            return new { };
        });

        RegisterMethodHandler(McpMethods.ToolsList, async (_) =>
        {
            return await GetToolsListAsync();
        });

        RegisterMethodHandler(McpMethods.ToolsCall, async (params_) =>
        {
            McpToolCallParams parameters = DeserializeParams<McpToolCallParams>(params_);
            return await CallToolAsync(parameters);
        });
    }

    private static T DeserializeParams<T>(object? params_)
    {
        if (params_ is JsonElement element)
        {
            return JsonSerializer.Deserialize<T>(element, Options)
                   ?? throw new ArgumentException($"Failed to deserialize parameters to {typeof(T).Name}");
        }

        if (params_ is T directParams)
        {
            return directParams;
        }

        throw new ArgumentException($"Invalid parameters type for {typeof(T).Name}");
    }
}
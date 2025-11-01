using McpCore.JsonRpc;
using McpCore.Protocol;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpCore.Server;

/// <summary>
/// Base implementation of MCP server
/// </summary>
public class McpServer : IMcpServer
{
    private readonly Dictionary<string, Func<object?, Task<object>>> _methodHandlers = new();
    private readonly McpServerInfo _serverInfo;
    private readonly McpServerCapabilities _capabilities;
    private bool _initialized;

    public McpServer(McpServerInfo serverInfo, McpServerCapabilities capabilities)
    {
        _serverInfo = serverInfo;
        _capabilities = capabilities;

        RegisterDefaultHandlers();
    }

    public static readonly JsonSerializerOptions options = new()
    {
        AllowOutOfOrderMetadataProperties = true,
        AllowTrailingCommas = true,
        Converters = {
                new JsonStringEnumConverter(allowIntegerValues: true),
            },
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        MaxDepth = 5,
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

    public virtual Task<McpToolsListResult> GetToolsListAsync()
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

    public virtual Task<McpToolCallResult> CallToolAsync(McpToolCallParams parameters)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Server not initialized");
        }

        throw new NotImplementedException("Tool call implementation must be provided by derived class");
    }

    protected virtual McpTool[] GetAvailableTools()
    {
        return Array.Empty<McpTool>();
    }

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
            return JsonSerializer.Deserialize<T>(element, options)
                   ?? throw new ArgumentException($"Failed to deserialize parameters to {typeof(T).Name}");
        }

        if (params_ is T directParams)
        {
            return directParams;
        }

        throw new ArgumentException($"Invalid parameters type for {typeof(T).Name}");
    }
}
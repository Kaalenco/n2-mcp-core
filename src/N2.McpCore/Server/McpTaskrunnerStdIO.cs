using McpCore.JsonRpc;
using McpCore.Server;

using System.Text.Json;

namespace N2.McpCore.Server;

public static class McpServerStdIO
{
    public static async Task<int> RunMcpServerAsync<T>(this IServiceProvider serviceProvider, TextReader input, TextWriter output, Action<string> consoleWriter) where T : IMcpServer
    {
        consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Starting MCP Server");

        if (serviceProvider == null)
        {
            consoleWriter?.Invoke("FATAL: Service provider not configured.");
            return 1;
        }
        if (input == null || output == null)
        {
            consoleWriter?.Invoke("FATAL: StdIO Not configured.");
            return 1;
        }

        if (serviceProvider.GetService(typeof(T)) is not IMcpServer mcpServer)
        {
            consoleWriter?.Invoke($"FATAL: Could not find MCP Server : {typeof(T)} or the service does not implement IMcpServer.");
            return 1;
        }

        consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Ready to accept requests...");

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            while (true)
            {
                string? line = await input.ReadLineAsync();
                if (line == null) // EOF
                {
                    consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Shutting down gracefully.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    JsonRpcRequest? request = JsonSerializer.Deserialize<JsonRpcRequest>(line, McpServer.Options);
                    if (request == null)
                    {
                        continue;
                    }

                    consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Received: {request.Method}");

                    JsonRpcResponse response = await mcpServer.ProcessRequestAsync(request);

                    // Only send response for requests (not notifications)
                    if (request.Id != null)
                    {
                        string responseJson = JsonSerializer.Serialize(response, McpServer.Options);
                        consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Sending response: {responseJson.Substring(0, Math.Min(200, responseJson.Length))}...");
                        await output.WriteLineAsync(responseJson);
                        await output.FlushAsync();
                    }
                    else
                    {
                        consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Notification processed (no response sent)");
                    }
                }
                catch (JsonException jsonEx)
                {
                    consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] JSON Parse Error: {jsonEx.Message}");
                    // Send JSON-RPC parse error response
                    JsonRpcResponse errorResponse = new()
                    {
                        Id = null,
                        Error = new JsonRpcError
                        {
                            Code = JsonRpcErrorCodes.ParseError,
                            Message = "Parse error: Invalid JSON"
                        }
                    };
                    string errorJson = JsonSerializer.Serialize(errorResponse, McpServer.Options);
                    await output.WriteLineAsync(errorJson);
                    await output.FlushAsync();
                }
                catch (Exception ex)
                {
                    consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Unexpected error processing request: {ex.GetType().Name}: {ex.Message}");
                    consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Stack: {ex.StackTrace}");
                    // Send JSON-RPC internal error response
                    JsonRpcResponse errorResponse = new()
                    {
                        Id = null,
                        Error = new JsonRpcError
                        {
                            Code = JsonRpcErrorCodes.InternalError,
                            Message = $"Internal error: {ex.Message}"
                        }
                    };
                    string errorJson = JsonSerializer.Serialize(errorResponse, McpServer.Options);
                    await output.WriteLineAsync(errorJson);
                    await output.FlushAsync();
                }
            }
        }
        catch (Exception ex)
        {
            consoleWriter?.Invoke($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Fatal error: {ex.Message}");
            return 1;
        }
#pragma warning restore CA1031 // Do not catch general exception types

        return 0;
    }
}
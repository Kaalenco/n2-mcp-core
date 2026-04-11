# N2.McpCore - Technical Guide

This library provides the foundational components for building MCP (Model Context Protocol) servers in .NET. It handles the JSON-RPC communication layer and MCP protocol compliance, allowing you to focus on implementing your domain-specific tools.

## Installation

Add the package to your project:

```bash
dotnet add package N2.Core.Abstractions
```

## Creating a Custom MCP Server

### 1. Inherit from McpServer

Create a class that inherits from `McpServer` and provide server information and capabilities:

```csharp
using McpCore.Server;
using McpCore.Protocol;

public class YourMcpServer : McpServer
{
    public YourMcpServer() : base(
        new McpServerInfo
        {
            Name = "your-server-name",
            Version = "1.0.0"
        },
        new McpServerCapabilities
        {
            Tools = new McpToolsCapability()
        })
    {
    }
}
```

### 2. Define Available Tools

Override `GetAvailableTools()` to return an array of tool definitions. Each tool must include:
- **Name**: Unique identifier for the tool
- **Description**: Human-readable description of what the tool does
- **InputSchema**: JSON Schema defining the tool's parameters

```csharp
protected override McpTool[] GetAvailableTools()
{
    return new[]
    {
        new McpTool
        {
            Name = "your_tool_name",
            Description = "Description of what this tool does",
            InputSchema = new McpInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, McpPropertyDefinition>
                {
                    ["param1"] = new McpPropertyDefinition
                    {
                        Type = "string",
                        Description = "Description of parameter 1"
                    },
                    ["param2"] = new McpPropertyDefinition
                    {
                        Type = "boolean",
                        Description = "Description of parameter 2",
                        Default = false
                    },
                    ["param3"] = new McpPropertyDefinition
                    {
                        Type = "integer",
                        Description = "Description of parameter 3"
                    }
                },
                Required = new[] { "param1" } // List required parameters
            }
        }
    };
}
```

### 3. Implement Tool Execution

Override `CallToolAsync()` to handle tool execution. Use a switch statement or pattern matching to route to specific tool handlers:

```csharp
protected override async Task<McpToolCallResult> CallToolAsync(McpToolCallParams parameters)
{
    try
    {
        var result = parameters.Name switch
        {
            "your_tool_name" => await HandleYourToolAsync(parameters.Arguments),
            _ => throw new ArgumentException($"Unknown tool: {parameters.Name}")
        };

        return new McpToolCallResult
        {
            Content = new[] { new McpContent { Type = "text", Text = result } },
            IsError = false
        };
    }
    catch (Exception ex)
    {
        return new McpToolCallResult
        {
            Content = new[] { new McpContent { Type = "text", Text = $"Error: {ex.Message}" } },
            IsError = true
        };
    }
}
```

## Working with Tool Arguments

Tool arguments arrive as `Dictionary<string, object?>` where values may be `JsonElement` or native types. Use the helper methods to safely extract typed values:

```csharp
private static string? GetStringArgument(Dictionary<string, object?> arguments, string key)
{
    if (!arguments.TryGetValue(key, out var value) || value == null)
        return null;

    if (value is JsonElement element)
        return element.GetString();

    return value.ToString();
}

private static bool GetBoolArgument(Dictionary<string, object?> arguments, string key)
{
    if (!arguments.TryGetValue(key, out var value) || value == null)
        return false;

    if (value is JsonElement element)
        return element.GetBoolean();

    if (value is bool boolValue)
        return boolValue;

    return bool.TryParse(value.ToString(), out var result) && result;
}

private static int? GetIntArgument(Dictionary<string, object?> arguments, string key)
{
    if (!arguments.TryGetValue(key, out var value) || value == null)
        return null;

    if (value is JsonElement element)
        return element.GetInt32();

    if (value is int intValue)
        return intValue;

    return int.TryParse(value.ToString(), out var result) ? result : null;
}
```

## Dependency Injection Integration

MCP servers often need access to business logic services. Accept `IServiceProvider` in your constructor:

```csharp
public class YourMcpServer : McpServer
{
    private readonly IServiceProvider _serviceProvider;

    public YourMcpServer(IServiceProvider serviceProvider)
        : base(
            new McpServerInfo { Name = "your-server", Version = "1.0.0" },
            new McpServerCapabilities { Tools = new McpToolsCapability() }
        )
    {
        _serviceProvider = serviceProvider;
    }

    private async Task<string> HandleYourToolAsync(Dictionary<string, object?> arguments)
    {
        var service = _serviceProvider.GetRequiredService<IYourService>();
        // Use the service to implement tool logic
        var result = await service.DoSomethingAsync();
        return result.Message;
    }
}
```

Register your server in the DI container:

```csharp
serviceCollection.AddSingleton<YourMcpServer>();
```

## Setting Up stdio Communication

MCP servers communicate via JSON-RPC 2.0 over standard input/output. Implement a loop that:
1. Reads JSON-RPC requests from `Console.In`
2. Processes them through your MCP server
3. Writes JSON-RPC responses to `Console.Out`
4. Logs diagnostic information to `Console.Error`

```csharp
private static async Task<int> RunMcp(IServiceProvider serviceProvider)
{
    serviceProvider.RunMcpServerAsync<YourMcpServer>(Console.In, Console.Out, Console.Error.Writeline);
}
```

## Utility Classes

### Response and Response&lt;T&gt;

The `Response` class provides a standardized way to return operation results:

```csharp
// Simple success/failure
var result = Response.Ok();
var failed = Response.Fail("Operation failed");

// With message
var success = Response.Ok("Operation completed successfully");

// With typed value
var data = Response.Ok("Data retrieved", new { Id = 1, Name = "Test" });
var typedData = Response.Ok(new User { Id = 1, Name = "John" });

// Check result
if (result.Success)
{
    Console.WriteLine(result.Message);
}
```

### VerifyResult

The `VerifyResult` class accumulates validation messages:

```csharp
var verify = VerifyResult.Start("Validating input");

if (string.IsNullOrEmpty(name))
{
    verify.Fail("Name is required");
}

if (age < 0)
{
    verify.Fail("Age must be positive");
}

verify.Remark("Validation completed");

// Throw exception if any failures occurred
verify.ThrowIfFailed();

// Or check manually
if (verify.Failure)
{
    foreach (var message in verify.Messages)
    {
        Console.WriteLine(message);
    }
}
```

### SessionCodeGenerator

Generate and validate 6-character session codes for client connections:

```csharp
var generator = new SessionCodeGenerator();

// Generate a code like "ABC123"
string code = generator.GenerateCode();

// Validate a code
bool isValid = generator.IsValidCode("ABC123"); // true
bool isInvalid = generator.IsValidCode("invalid"); // false
```

Register in DI container:

```csharp
serviceCollection.AddSingleton<ISessionCodeGenerator, SessionCodeGenerator>();
```

## JSON Serialization

The library provides pre-configured `JsonSerializerOptions` via `McpServer.options`:

```csharp
var json = JsonSerializer.Serialize(data, McpServer.options);
var obj = JsonSerializer.Deserialize<MyType>(json, McpServer.options);
```

Features:
- Case-insensitive property names
- Enum string conversion with integer fallback
- Trailing commas allowed
- Number reading from strings
- Out-of-order metadata properties
- Maximum depth of 5

## Best Practices

### Input Validation
- Always validate required parameters and throw `ArgumentException` for missing or invalid inputs
- Use `VerifyResult` to accumulate validation errors before throwing
- Use try-catch in `CallToolAsync()` to return structured error responses
- Provide clear error messages to help users understand what went wrong

### Tool Design
- Use clear, descriptive tool names following `category_action` pattern (e.g., `file_read`, `data_transform`)
- Write detailed descriptions that explain what the tool does and when to use it
- Define comprehensive input schemas with descriptions for all parameters
- Mark parameters as required or provide sensible defaults

### Error Handling
- Catch exceptions at the tool execution level
- Return `McpToolCallResult` with `IsError = true` for failures
- Include meaningful error messages in the response content
- Log detailed error information to `Console.Error` for debugging

### Performance
- Keep tool execution lightweight and responsive
- For long-running operations, consider returning progress updates
- Dispose of resources properly in async operations

### Logging
- Use `Console.Error` for all diagnostic logging (never `Console.Out`)
- Include timestamps in log messages
- Log request/response counts and timing information
- Log errors with full exception details

### MCP Protocol Compliance
- Never modify the base `McpServer` initialization flow
- Respect the two-phase initialization (initialize → initialized notification)
- Handle notifications (requests without IDs) correctly by not sending responses
- Use standard JSON-RPC error codes from `JsonRpcErrorCodes`

## Testing Your Server

Create unit tests that verify:
1. Tool definitions are correctly formatted
2. Tool execution handles valid inputs correctly
3. Error cases return appropriate error responses
4. Required parameters are validated

```csharp
[Test]
public async Task CallToolAsync_WithValidInput_ReturnsSuccess()
{
    var server = new YourMcpServer(serviceProvider);
    var result = await server.CallToolAsync(new McpToolCallParams
    {
        Name = "your_tool_name",
        Arguments = new Dictionary<string, object?>
        {
            ["param1"] = "test value"
        }
    });

    Assert.IsFalse(result.IsError);
    Assert.IsNotEmpty(result.Content);
}
```

## Common Patterns

### Returning Multiple Content Items
You can return multiple content items in a single response:

```csharp
return new McpToolCallResult
{
    Content = new[]
    {
        new McpContent { Type = "text", Text = "Summary information" },
        new McpContent { Type = "text", Text = "Detailed data", MimeType = "application/json" }
    },
    IsError = false
};
```

### Handling Enum Parameters
For parameters with limited valid values, use enums and parse them from strings:

```csharp
var statusStr = GetStringArgument(arguments, "status") ?? "Default";
var status = Enum.TryParse<YourEnum>(statusStr, true, out var parsed)
    ? parsed
    : throw new ArgumentException($"Invalid status: {statusStr}");
```

### Using Response Pattern for Tool Results
Combine `Response` class with tool execution:

```csharp
private async Task<string> HandleYourToolAsync(Dictionary<string, object?> arguments)
{
    var service = _serviceProvider.GetRequiredService<IYourService>();
    var result = await service.DoSomethingAsync();

    return result.Success
        ? result.Message ?? "Operation completed successfully"
        : $"Failed: {result.Message}";
}
```

## Changelog

### 1.1.1
- Fixed: `McpTool.CallAsync` is now decorated with `[JsonIgnore]` so that `tools/list` responses serialize correctly. Previously the `Func<>` delegate caused a `NotSupportedException` at runtime when the JSON serializer encountered the property.

### 1.1.0
- Initial public release.

## Support and Resources

- [MCP Specification](https://modelcontextprotocol.io/)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)
- [Project Repository](https://github.com/kaalenco/n2-mcp-core)

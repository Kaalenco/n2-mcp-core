# N2.McpCore

[![.NET Build and test](https://github.com/Kaalenco/n2-mcp-core/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Kaalenco/n2-mcp-core/actions/workflows/dotnet.yml)

A .NET library providing core abstractions and utilities for implementing [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) servers. This package enables you to build MCP-compliant servers that can integrate with AI assistants and other MCP clients.

## Features

- **Multi-Framework Support**: Targets .NET Standard 2.0, .NET Standard 2.1, .NET 8.0, and .NET 10.0
- **JSON-RPC 2.0 Implementation**: Complete JSON-RPC protocol support for request/response handling
- **MCP Protocol Compliance**: Implements MCP specification version "2024-11-05"
- **Extensible Architecture**: Base classes and interfaces for easy server implementation
- **Type-Safe Models**: Strongly-typed protocol models with JSON serialization support
- **Tool Framework**: Built-in support for defining and executing MCP tools

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package N2.Mcp.Core
```

Or via Package Manager Console:

```powershell
Install-Package N2.Mcp.Core
```

## Quick Start

### 1. Create a Custom MCP Server

Each tool now carries its own `CallAsync` delegate, so you no longer need to override `CallToolAsync`. Assign the handler directly on the `McpTool` definition:

```csharp
using McpCore.Server;
using McpCore.Protocol;

public class MyMcpServer : McpServer
{
    public MyMcpServer() : base(
        new McpServerInfo { Name = "my-server", Version = "1.0.0" },
        new McpServerCapabilities { Tools = new McpToolsCapability() })
    {
    }

    protected override McpTool[] GetAvailableTools() =>
    [
        new McpTool
        {
            Name = "get_weather",
            Description = "Get current weather for a location",
            CallAsync = GetWeatherAsync,
            InputSchema = new McpInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, McpPropertyDefinition>
                {
                    ["location"] = new McpPropertyDefinition
                    {
                        Type = "string",
                        Description = "City name"
                    }
                },
                Required = ["location"]
            }
        }
    ];

    private Task<McpToolCallResult> GetWeatherAsync(Dictionary<string, object?> arguments)
    {
        var location = arguments.GetStringArgument("location") ?? "unknown";
        return Task.FromResult(new McpToolCallResult
        {
            Content =
            [
                new McpContent { Type = "text", Text = $"Weather in {location}: Sunny, 72°F" }
            ]
        });
    }
}
```

### 2. Process JSON-RPC Requests

```csharp
var server = new MyMcpServer();

// Handle incoming JSON-RPC request
var request = new JsonRpcRequest
{
    Id = 1,
    Method = "tools/list",
    Params = null
};

var response = await server.ProcessRequestAsync(request);
```

## Architecture

The library is organized into three main components:

- **JsonRpc**: JSON-RPC 2.0 protocol implementation with request/response/error models
- **Protocol**: MCP-specific protocol models including initialization, tools, and capabilities
- **Server**: Base server implementation with extensible method handlers

### MCP Server Lifecycle

1. **Initialize**: Client sends `initialize` request with capabilities
2. **Initialized**: Client sends `initialized` notification to complete handshake
3. **Ready**: Server can now handle `tools/list` and `tools/call` requests

For detailed architecture documentation, see [CLAUDE.md](CLAUDE.md).

## Building from Source

```bash
# Clone the repository
git clone https://github.com/kaalenco/n2-mcp-core.git
cd n2-mcp-core

# Build the solution
dotnet build src/N2.McpCore.sln

# Build for release
dotnet build src/N2.McpCore.sln -c Release
```

## Contributing

Contributions are welcome! Please ensure:

- Code follows existing architectural patterns
- All code analysis warnings are addressed (warnings are treated as errors)
- XML documentation is provided for public APIs
- Changes are tested across all target frameworks

## License

This project is licensed under the Academic Free License v3.0 (AFL-3.0). See [LICENSE](LICENSE) for details.

Copyright (c) 2025 Kaalenco - All rights reserved

## Changelog

### 1.1.0

**Breaking changes**

- `GetAvailableTools()` is now `abstract`. Subclasses must implement it; the default empty-array implementation has been removed.
- `CallToolAsync()` is no longer `virtual`. Overriding it has no effect. Tool dispatch is now handled internally by `McpServer` via the `McpTools` dictionary populated during the `initialized` handshake.
- Servers that return an empty tool array from `GetAvailableTools()` will now throw `InvalidOperationException` during the `initialized` handshake. At least one tool must be registered.

**New features**

- `McpTool.CallAsync` — new `Func<Dictionary<string, object?>, Task<McpToolCallResult>>?` delegate property. Assign the tool's handler directly on the `McpTool` definition instead of a central `switch` in `CallToolAsync`.
- Tool lookup is **case-insensitive** and tool names are **trimmed** before lookup, reducing sensitivity to minor client variations.
- Descriptive errors: calling a tool that has no `CallAsync` assigned throws `InvalidOperationException("Tool 'x' is not callable")`.
- `CallToolAsync` now validates that `parameters` is not `null` before proceeding.
- Unit test project added (`N2.McpCore.Unittests`, NUnit, targets `net8.0` and `net10.0`).
- Target frameworks updated: `.NET 9.0` replaced by `.NET 10.0`.

**Migration from 1.0.x**

1. Remove any `override` of `CallToolAsync` in your server subclass.
2. Add a `CallAsync` delegate to each `McpTool` returned by `GetAvailableTools()`.
3. Ensure `GetAvailableTools()` returns at least one tool, or guard the `initialized` flow accordingly.

---

### 1.0.1 — 1.0.0

Initial release with JSON-RPC 2.0 foundation, MCP protocol models, `McpServer` base class, and stdio transport helper.

---

## Links

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Package on NuGet](https://www.nuget.org/packages/N2.Mcp.Core)
- [Project Repository](https://github.com/kaalenco/n2-mcp-core)
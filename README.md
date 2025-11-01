# N2.McpCore

A .NET library providing core abstractions and utilities for implementing [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) servers. This package enables you to build MCP-compliant servers that can integrate with AI assistants and other MCP clients.

## Features

- **Multi-Framework Support**: Targets .NET Standard 2.0, .NET Standard 2.1, .NET 8.0, and .NET 9.0
- **JSON-RPC 2.0 Implementation**: Complete JSON-RPC protocol support for request/response handling
- **MCP Protocol Compliance**: Implements MCP specification version "2024-11-05"
- **Extensible Architecture**: Base classes and interfaces for easy server implementation
- **Type-Safe Models**: Strongly-typed protocol models with JSON serialization support
- **Tool Framework**: Built-in support for defining and executing MCP tools

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package N2.Core.Abstractions
```

Or via Package Manager Console:

```powershell
Install-Package N2.Core.Abstractions
```

## Quick Start

### 1. Create a Custom MCP Server

```csharp
using McpCore.Server;
using McpCore.Protocol;

public class MyMcpServer : McpServer
{
    public MyMcpServer() : base(
        new McpServerInfo
        {
            Name = "my-server",
            Version = "1.0.0"
        },
        new McpServerCapabilities
        {
            Tools = new McpToolsCapability()
        })
    {
    }

    protected override McpTool[] GetAvailableTools()
    {
        return new[]
        {
            new McpTool
            {
                Name = "get_weather",
                Description = "Get current weather for a location",
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
                    Required = new[] { "location" }
                }
            }
        };
    }

    protected override async Task<McpToolCallResult> CallToolAsync(McpToolCallParams parameters)
    {
        if (parameters.Name == "get_weather")
        {
            var location = parameters.Arguments["location"]?.ToString();
            // Implement your tool logic here
            return new McpToolCallResult
            {
                Content = new[]
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = $"Weather in {location}: Sunny, 72°F"
                    }
                }
            };
        }

        throw new ArgumentException($"Unknown tool: {parameters.Name}");
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

## Configuration

The library includes flexible configuration utilities through `N2ConfigurationExtensions`:

```csharp
// Check if a feature is enabled from IConfiguration
bool isEnabled = configuration.HasFeature("MyFeature");
```

Supported configuration formats:
- Boolean flags: `Features:MyFeature = true`
- CSV strings: `Features = "FeatureA,FeatureB"`
- String arrays: `Features = ["FeatureA", "FeatureB"]`

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

## Links

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Package on NuGet](https://www.nuget.org/packages/N2.Core.Abstractions)
- [Project Repository](https://github.com/kaalenco/n2-core-abstractions)
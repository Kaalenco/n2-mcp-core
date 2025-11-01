# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

N2.McpCore is a .NET library that provides core abstractions and utilities for implementing MCP (Model Context Protocol) servers. This package targets multiple frameworks (netstandard2.0, netstandard2.1, .NET 8.0, and .NET 9.0) to maximize compatibility across different .NET projects.

## Build and Development Commands

### Building the Project
```bash
# Build the solution
dotnet build src/N2.McpCore.sln

# Build for specific configuration
dotnet build src/N2.McpCore.sln -c Release

# Build for specific framework
dotnet build src/N2.McpCore.sln -f net9.0
```

### Package Creation
The project is configured to generate NuGet packages on build (`GeneratePackageOnBuild` is enabled). The package will be created in the `bin` directory after a successful build.

### Testing
Currently, no test project is present in this repository. When adding tests, they should be placed in a separate test project.

## Architecture Overview

### Core Components

The codebase is organized into three main layers:

1. **JSON-RPC Layer** (`JsonRpc/`)
   - Provides JSON-RPC 2.0 protocol implementation
   - `JsonRpcRequest`, `JsonRpcResponse`, `JsonRpcNotification` - Core message types
   - `JsonRpcError` and `JsonRpcErrorCodes` - Standardized error handling
   - All JSON-RPC models follow the JSON-RPC 2.0 specification

2. **MCP Protocol Layer** (`Protocol/`)
   - Implements the Model Context Protocol (MCP) version "2024-11-05"
   - `McpInitializeParams` and `McpInitializeResult` - Server initialization handshake
   - `McpTool`, `McpToolCallParams`, `McpToolCallResult` - Tool definition and execution
   - `McpServerInfo` and `McpServerCapabilities` - Server metadata and capabilities
   - `McpMethods` - Standard MCP method name constants (initialize, tools/list, tools/call)
   - Uses JSON Schema for tool input validation via `McpInputSchema`

3. **Server Implementation** (`Server/`)
   - `IMcpServer` - Interface defining MCP server functionality
   - `McpServer` - Base implementation providing:
     - JSON-RPC request routing to registered method handlers
     - MCP initialization handshake (initialize → initialized notification → ready)
     - Tool listing and invocation framework
     - Error handling and protocol compliance
   - Derived classes must override `GetAvailableTools()` and `CallToolAsync()` to provide actual tool implementations

### Key Patterns

**Initialization Flow:**
The MCP server follows a two-phase initialization:
1. Client sends `initialize` request → Server responds with capabilities
2. Client sends `initialized` notification → Server marks itself ready (sets `_initialized = true`)
3. Only after phase 2 can tools be listed or called

**Method Handler Registration:**
`McpServer` uses a dictionary-based dispatcher (`_methodHandlers`) where each MCP method is registered with an async handler function. This allows easy extension by calling `RegisterMethodHandler()`.

**Configuration:**
`N2ConfigurationExtensions` provides flexible feature flag checking supporting boolean flags, CSV strings, and string arrays from IConfiguration. This is used for runtime feature toggles.

**JSON Serialization:**
The library defines standardized `JsonSerializerOptions` in `N2ConfigurationExtensions.options` with:
- Case-insensitive property names
- Enum string conversion
- Trailing commas allowed
- Number reading from strings
- Max depth of 5

## Important Implementation Notes

- The server uses `Task.Delay(1)` in the `initialized` handler as a placeholder (src/N2.McpCore/Server/McpServer.cs:134) - this is part of the notification handling pattern
- Tool calls and tool listing will throw `InvalidOperationException` if called before the server is initialized
- Parameter deserialization supports both `JsonElement` and direct type parameters for flexibility
- All MCP protocol models use record types for immutability and value equality
- The codebase enforces strict code analysis with warnings as errors and multiple analyzers enabled

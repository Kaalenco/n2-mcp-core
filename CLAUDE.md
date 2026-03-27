# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

N2.McpCore is a .NET library that provides core abstractions and utilities for implementing MCP (Model Context Protocol) servers. The package targets `netstandard2.0`, `netstandard2.1`, `net8.0`, and `net10.0`.

## Build and Development Commands

```bash
# Build the solution
dotnet build src/N2.McpCore.sln

# Build for specific configuration
dotnet build src/N2.McpCore.sln -c Release

# Run tests (NUnit, targets net8.0 and net10.0)
dotnet test src/N2.McpCore.Unittests/N2.McpCore.Unittests.csproj

# Run a single test
dotnet test src/N2.McpCore.Unittests/N2.McpCore.Unittests.csproj --filter "FullyQualifiedName~TestName"
```

NuGet packages are generated automatically on build (`GeneratePackageOnBuild` is enabled), output to `bin/`.

## Architecture Overview

### Core Components

1. **JSON-RPC Layer** (`JsonRpc/`)
   - `JsonRpcRequest`, `JsonRpcResponse`, `JsonRpcNotification` — core message types
   - `JsonRpcError` and `JsonRpcErrorCodes` — standardized error handling per JSON-RPC 2.0 spec

2. **MCP Protocol Layer** (`Protocol/`)
   - Protocol version `"2024-11-05"`
   - `McpInitializeParams`/`McpInitializeResult` — handshake types
   - `McpTool`, `McpToolCallParams`, `McpToolCallResult`, `McpContent` — tool definition and execution
   - `McpServerInfo`, `McpServerCapabilities`, `McpToolsCapability` — server metadata
   - `McpMethods` — method name constants (`initialize`, `initialized`, `tools/list`, `tools/call`)
   - `McpInputSchema`, `McpPropertyDefinition` — JSON Schema for tool input validation

3. **Server Implementation** (`Server/`)
   - `IMcpServer` — interface defining `ProcessRequestAsync`, `InitializeAsync`, `GetToolsListAsync`, `CallToolAsync`
   - `McpServer` — base class; subclass by overriding `GetAvailableTools()` and `CallToolAsync()`
   - `McpServerStdIO` — static extension method `RunMcpServerAsync<T>(IServiceProvider, TextReader, TextWriter, Action<string>)` that runs the stdio message loop; resolves the server from DI and handles JSON-RPC framing, EOF, and error responses

4. **Utility Types** (root of `N2.McpCore`)
   - `DictionaryExtensions` — helpers for extracting typed arguments from `Dictionary<string, object?>` in tool implementations (`GetStringArgument`, `GetBoolArgument`, `GetIntArgument`, `GetArgument<T>`)
   - `Response` / `Response<T>` — generic operation result wrappers with `Success`, `Message`, `Value`
   - `VerifyResult` — accumulates validation failures; call `ThrowIfFailed()` to raise on error
   - `SessionCodeGenerator` / `ISessionCodeGenerator` — generates/validates 6-character alphanumeric session codes
   - `ToolCallRequest` — lightweight model for tool call name + `JsonElement?` arguments

### Key Patterns

**Initialization Flow (two-phase):**
1. Client sends `initialize` → server returns capabilities (does NOT set `_initialized`)
2. Client sends `initialized` notification → server sets `_initialized = true`
3. `GetToolsListAsync()` and `CallToolAsync()` throw `InvalidOperationException` before phase 2 completes

**Method Handler Registration:**
`McpServer` uses a dictionary dispatcher (`_methodHandlers`). Extend by calling `RegisterMethodHandler(method, handler)` in a constructor or override.

**JSON Serialization:**
`McpServer.Options` is the shared `JsonSerializerOptions` used throughout (case-insensitive, enum-as-string, trailing commas, null-ignoring, max depth 32). Pass it explicitly when serializing/deserializing outside of `McpServer`.

**Argument Extraction in Tools:**
Tool arguments arrive as `Dictionary<string, object?>` where values are `JsonElement` (from deserialization) or CLR types (from tests/direct calls). Use `DictionaryExtensions` helpers to handle both cases uniformly.

### Reference Implementation

`src/N2.McpCore.Calculator/` is the canonical example — `CalculatorServer` extends `McpServer`, overrides `GetAvailableTools()` returning `McpTool[]` with JSON Schema, and overrides `CallToolAsync()` dispatching by name. `Program.cs` shows how to wire up DI and call `RunMcpServerAsync`.

## Code Analysis

The library enforces strict Roslyn analysis (`AnalysisMode=All`, `CodeAnalysisTreatWarningsAsErrors=true`). Suppressed warnings are listed in `NoWarn` in the `.csproj`. New code must pass all enabled analyzers.

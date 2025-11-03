# Simple Calculator MCP Server

A simple example MCP (Model Context Protocol) server implementation that provides basic arithmetic operations.

## Overview

This project demonstrates how to build an MCP server using the N2.McpCore library. It implements a calculator service with five tools:

- **add** - Adds two numbers
- **subtract** - Subtracts two numbers
- **multiply** - Multiplies two numbers
- **divide** - Divides two numbers (with division by zero protection)
- **how-to-use-this-server** - Returns documentation about the server

## Building and Running

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

The server communicates via standard input/output (stdio) using JSON-RPC 2.0 messages following the MCP protocol.

## Architecture

The calculator server is built on top of the `McpServer` base class from N2.McpCore:

- **CalculatorServer.cs** - Implements the MCP server by:
  - Overriding `GetAvailableTools()` to define the five calculator tools
  - Overriding `CallToolAsync()` to handle tool execution
  - Providing JSON Schema definitions for tool parameters

- **Program.cs** - Entry point that:
  - Sets up dependency injection
  - Registers the CalculatorServer
  - Launches the server using `McpServerStdIO.RunMcpServerAsync()`

## MCP Protocol Flow

1. Client sends `initialize` request
2. Server responds with capabilities and server info
3. Client sends `initialized` notification
4. Client can now call `tools/list` to get available tools
5. Client calls `tools/call` with tool name and arguments
6. Server executes tool and returns result

## Example Tool Call

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "add",
    "arguments": {
      "a": 5,
      "b": 3
    }
  }
}
```

Response:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "5 + 3 = 8"
      }
    ],
    "isError": false
  }
}
```

## Testing with MCP Inspector

### What is MCP Inspector?

The MCP Inspector is an interactive, browser-based developer tool provided by the Model Context Protocol team for testing and debugging MCP servers. It provides a visual interface to:

- Connect to and initialize MCP servers
- View and test available tools
- Inspect tool schemas and parameter requirements
- Execute tools with custom inputs and view results
- Monitor server logs and notifications
- Test error handling and edge cases

### Installation

No installation required! The Inspector runs directly via npx:

```bash
npx @modelcontextprotocol/inspector <command>
```

### Testing the Calculator Server

From the repository root, run:

```bash
npx @modelcontextprotocol/inspector dotnet run --project src/N2.McpCore.Calculator
```

This will:
1. Build and start the calculator server
2. Launch the MCP Inspector web interface (typically at http://localhost:6274)
3. Automatically connect to your server
4. Display a session token for security

### Using the Inspector Interface

Once the Inspector opens in your browser:

1. **Connection Tab**
   - Verify the server initialized successfully
   - Check server info (name: "simple-calculator", version: "1.0.0")
   - Review capabilities (should show tools support)

2. **Tools Tab**
   - View all 5 available tools (add, subtract, multiply, divide, how-to-use-this-server)
   - Inspect the JSON Schema for each tool's parameters
   - Test tool execution:
     - Select a tool (e.g., "add")
     - Enter arguments in the form or JSON editor (e.g., `{"a": 10, "b": 5}`)
     - Click "Execute" to run the tool
     - View the result in the response panel

3. **Notifications Pane**
   - Monitor server diagnostic messages
   - View server logs written to stderr
   - Track initialization sequence

### Example Testing Workflow

1. **Test Basic Operations**
   ```
   Tool: add
   Arguments: {"a": 100, "b": 50}
   Expected: "100 + 50 = 150"
   ```

2. **Test Division by Zero**
   ```
   Tool: divide
   Arguments: {"a": 10, "b": 0}
   Expected: Error response with "Division by zero is not allowed"
   ```

3. **Test Server Documentation**
   ```
   Tool: how-to-use-this-server
   Arguments: {} (empty object)
   Expected: Full documentation text
   ```

4. **Test Invalid Parameters**
   - Try missing required parameters
   - Try non-numeric values
   - Verify appropriate error messages

### Alternative Testing Methods

#### Manual JSON-RPC via stdin/stdout

You can also test by running the server directly and sending JSON-RPC messages:

```bash
dotnet run --project src/N2.McpCore.Calculator
```

Then send messages like:
```json
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0.0"}}}
{"jsonrpc":"2.0","method":"notifications/initialized","params":{}}
{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"add","arguments":{"a":5,"b":3}}}
```

#### Other MCP Testing Tools

- **Online MCP Inspector**: Visit https://onlinemcpinspector.com/ for a web-based inspector
- **MCP Tools CLI**: Command-line interface for stdio-based testing
- **mcpjam**: Connect to real AI models (Claude, GPT) for full conversational testing

### Troubleshooting

If the Inspector fails to connect:
- Ensure the calculator project builds successfully (`dotnet build`)
- Check that .NET 9.0 SDK is installed
- Verify the project path is correct relative to where you run the command
- Check stderr output for server initialization errors

## Dependencies

- .NET 9.0
- N2.McpCore (local project reference)
- Microsoft.Extensions.DependencyInjection

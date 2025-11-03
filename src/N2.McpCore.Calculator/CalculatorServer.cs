using McpCore.Protocol;
using McpCore.Server;
using System.Text.Json;

namespace N2.McpCore.Calculator;

/// <summary>
/// Simple calculator MCP server that provides basic arithmetic operations
/// </summary>
public class CalculatorServer : McpServer
{
    public CalculatorServer()
        : base(
            new McpServerInfo
            {
                Name = "simple-calculator",
                Version = "1.0.0"
            },
            new McpServerCapabilities
            {
                Tools = new McpToolsCapability
                {
                    ListChanged = false
                }
            })
    {
    }

    protected override McpTool[] GetAvailableTools()
    {
        return new[]
        {
            new McpTool
            {
                Name = "add",
                Description = "Add two numbers together",
                InputSchema = new McpInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, McpPropertyDefinition>
                    {
                        ["a"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "First number"
                        },
                        ["b"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "Second number"
                        }
                    },
                    Required = new[] { "a", "b" }
                }
            },
            new McpTool
            {
                Name = "subtract",
                Description = "Subtract the second number from the first number",
                InputSchema = new McpInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, McpPropertyDefinition>
                    {
                        ["a"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "Number to subtract from"
                        },
                        ["b"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "Number to subtract"
                        }
                    },
                    Required = new[] { "a", "b" }
                }
            },
            new McpTool
            {
                Name = "multiply",
                Description = "Multiply two numbers together",
                InputSchema = new McpInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, McpPropertyDefinition>
                    {
                        ["a"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "First number"
                        },
                        ["b"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "Second number"
                        }
                    },
                    Required = new[] { "a", "b" }
                }
            },
            new McpTool
            {
                Name = "divide",
                Description = "Divide the first number by the second number",
                InputSchema = new McpInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, McpPropertyDefinition>
                    {
                        ["a"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "Dividend (number to be divided)"
                        },
                        ["b"] = new McpPropertyDefinition
                        {
                            Type = "number",
                            Description = "Divisor (number to divide by)"
                        }
                    },
                    Required = new[] { "a", "b" }
                }
            },
            new McpTool
            {
                Name = "how-to-use-this-server",
                Description = "Get information about how to use this calculator server",
                InputSchema = new McpInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, McpPropertyDefinition>(),
                    Required = Array.Empty<string>()
                }
            }
        };
    }

    public override async Task<McpToolCallResult> CallToolAsync(McpToolCallParams parameters)
    {
        return parameters.Name switch
        {
            "add" => await ExecuteAddAsync(parameters.Arguments),
            "subtract" => await ExecuteSubtractAsync(parameters.Arguments),
            "multiply" => await ExecuteMultiplyAsync(parameters.Arguments),
            "divide" => await ExecuteDivideAsync(parameters.Arguments),
            "how-to-use-this-server" => await GetServerInfoAsync(),
            _ => new McpToolCallResult
            {
                Content = new[]
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = $"Unknown tool: {parameters.Name}"
                    }
                },
                IsError = true
            }
        };
    }

    private Task<McpToolCallResult> ExecuteAddAsync(Dictionary<string, object?> arguments)
    {
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");
        double result = a + b;

        return Task.FromResult(new McpToolCallResult
        {
            Content = new[]
            {
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} + {b} = {result}"
                }
            }
        });
    }

    private Task<McpToolCallResult> ExecuteSubtractAsync(Dictionary<string, object?> arguments)
    {
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");
        double result = a - b;

        return Task.FromResult(new McpToolCallResult
        {
            Content = new[]
            {
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} - {b} = {result}"
                }
            }
        });
    }

    private Task<McpToolCallResult> ExecuteMultiplyAsync(Dictionary<string, object?> arguments)
    {
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");
        double result = a * b;

        return Task.FromResult(new McpToolCallResult
        {
            Content = new[]
            {
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} × {b} = {result}"
                }
            }
        });
    }

    private Task<McpToolCallResult> ExecuteDivideAsync(Dictionary<string, object?> arguments)
    {
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");

        if (Math.Abs(b) < double.Epsilon)
        {
            return Task.FromResult(new McpToolCallResult
            {
                Content = new[]
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = "Error: Division by zero is not allowed"
                    }
                },
                IsError = true
            });
        }

        double result = a / b;

        return Task.FromResult(new McpToolCallResult
        {
            Content = new[]
            {
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} ÷ {b} = {result}"
                }
            }
        });
    }

    private Task<McpToolCallResult> GetServerInfoAsync()
    {
        string info = @"# Simple Calculator MCP Server

## Overview
This is a simple calculator server that provides basic arithmetic operations via the Model Context Protocol (MCP).

## Available Tools

### 1. add
Adds two numbers together.
- Parameters: a (number), b (number)
- Returns: The sum of a and b

### 2. subtract
Subtracts the second number from the first.
- Parameters: a (number), b (number)
- Returns: The difference (a - b)

### 3. multiply
Multiplies two numbers together.
- Parameters: a (number), b (number)
- Returns: The product of a and b

### 4. divide
Divides the first number by the second.
- Parameters: a (number), b (number)
- Returns: The quotient (a / b)
- Note: Division by zero returns an error

### 5. how-to-use-this-server
Displays this help information.
- Parameters: none
- Returns: Documentation about the server

## Example Usage
To use this server with an MCP client:
1. Initialize the connection using the MCP initialize handshake
2. List available tools using 'tools/list'
3. Call tools using 'tools/call' with the tool name and required arguments

## Server Information
- Name: simple-calculator
- Version: 1.0.0
- Protocol: MCP 2024-11-05";

        return Task.FromResult(new McpToolCallResult
        {
            Content = new[]
            {
                new McpContent
                {
                    Type = "text",
                    Text = info
                }
            }
        });
    }

    private static double GetNumberArgument(Dictionary<string, object?> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out object? value))
        {
            throw new ArgumentException($"Missing required argument: {name}");
        }

        // Handle JsonElement (from deserialization)
        if (value is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDouble();
            }
            throw new ArgumentException($"Argument '{name}' must be a number");
        }

        // Handle direct numeric types
        return value switch
        {
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            decimal dec => (double)dec,
            _ => throw new ArgumentException($"Argument '{name}' must be a number")
        };
    }
}

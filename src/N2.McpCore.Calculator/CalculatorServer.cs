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
                Version = "1.0.1"
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
        return
        [
            new McpTool
            {
                Name = "add",
                CallAsync = ExecuteAddAsync,
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
                    Required = ["a", "b"]
                }
            },
            new McpTool
            {
                Name = "subtract",
                CallAsync = ExecuteSubtractAsync,
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
                    Required = ["a", "b"]
                }
            },
            new McpTool
            {
                Name = "multiply",
                CallAsync = ExecuteMultiplyAsync,
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
                    Required = ["a", "b"]
                }
            },
            new McpTool
            {
                Name = "divide",
                CallAsync = ExecuteDivideAsync,
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
                    Required = ["a", "b"]
                }
            },
            new McpTool
            {
                Name = "how-to-use-this-server",
                CallAsync = GetServerInfoAsync,
                Description = "Get information about how to use this calculator server",
                InputSchema = new McpInputSchema
                {
                    Type = "object",
                    Properties = [],
                    Required = []
                }
            }
        ];
    }

    private Task<McpToolCallResult> ExecuteAddAsync(Dictionary<string, object?> arguments)
    {
        ValidateNumberArguments(arguments, "a", "b");
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");
        double result = a + b;

        return Task.FromResult(new McpToolCallResult
        {
            Content =
            [
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} + {b} = {result}"
                }
            ]
        });
    }

    private Task<McpToolCallResult> ExecuteSubtractAsync(Dictionary<string, object?> arguments)
    {
        ValidateNumberArguments(arguments, "a", "b");
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");
        double result = a - b;

        return Task.FromResult(new McpToolCallResult
        {
            Content =
            [
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} - {b} = {result}"
                }
            ]
        });
    }

    private Task<McpToolCallResult> ExecuteMultiplyAsync(Dictionary<string, object?> arguments)
    {
        ValidateNumberArguments(arguments, "a", "b");
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");
        double result = a * b;

        return Task.FromResult(new McpToolCallResult
        {
            Content =
            [
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} × {b} = {result}"
                }
            ]
        });
    }

    private Task<McpToolCallResult> ExecuteDivideAsync(Dictionary<string, object?> arguments)
    {
        ValidateNumberArguments(arguments, "a", "b");
        double a = GetNumberArgument(arguments, "a");
        double b = GetNumberArgument(arguments, "b");

        if (Math.Abs(b) < double.Epsilon)
        {
            return Task.FromResult(new McpToolCallResult
            {
                Content =
                [
                    new McpContent
                    {
                        Type = "text",
                        Text = "Error: Division by zero is not allowed"
                    }
                ],
                IsError = true
            });
        }

        double result = a / b;

        return Task.FromResult(new McpToolCallResult
        {
            Content =
            [
                new McpContent
                {
                    Type = "text",
                    Text = $"{a} ÷ {b} = {result}"
                }
            ]
        });
    }

    private Task<McpToolCallResult> GetServerInfoAsync(Dictionary<string, object?> arguments)
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
- Version: 1.0.1
- Protocol: MCP 2024-11-05";

        return Task.FromResult(new McpToolCallResult
        {
            Content =
            [
                new McpContent
                {
                    Type = "text",
                    Text = info
                }
            ]
        });
    }

    /// <summary>
    /// Validates that all required number arguments are present and numeric,
    /// accumulating every problem before throwing so the caller sees all errors at once.
    /// </summary>
    private static void ValidateNumberArguments(Dictionary<string, object?> arguments, params string[] names)
    {
        var verify = VerifyResult.Start("Validating calculator arguments");

        foreach (var name in names)
        {
            if (!arguments.TryGetValue(name, out object? value) || value == null)
            {
                verify.Fail($"Missing required argument: '{name}'");
                continue;
            }

            bool isNumber = value is JsonElement el
                ? el.ValueKind == JsonValueKind.Number
                : value is double or float or int or long or decimal;

            if (!isNumber)
            {
                verify.Fail($"Argument '{name}' must be a number");
            }
        }

        if (verify.Success)
            verify.Complete(null);
        else
            verify.CompleteWithFailure(null);

        verify.ThrowIfFailed();
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

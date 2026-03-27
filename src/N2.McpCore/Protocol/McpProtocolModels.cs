using System.Text.Json.Serialization;

namespace McpCore.Protocol;

/// <summary>
/// MCP initialize request parameters
/// </summary>
public record McpInitializeParams
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpClientCapabilities? Capabilities { get; set; }

    [JsonPropertyName("clientInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpClientInfo? ClientInfo { get; set; }
}

/// <summary>
/// MCP client information
/// </summary>
public record McpClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// MCP client capabilities
/// </summary>
public record McpClientCapabilities
{
    [JsonPropertyName("experimental")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Experimental { get; set; }
}

/// <summary>
/// MCP initialize response result
/// </summary>
public record McpInitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public McpServerCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("serverInfo")]
    public McpServerInfo ServerInfo { get; set; } = new();
}

/// <summary>
/// MCP server capabilities
/// </summary>
public record McpServerCapabilities
{
    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public McpToolsCapability? Tools { get; set; } = new();

    [JsonPropertyName("experimental")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Experimental { get; set; }
}

/// <summary>
/// MCP tools capability
/// </summary>
public record McpToolsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; } = false;
}

/// <summary>
/// MCP server information
/// </summary>
public record McpServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// MCP tool call parameters
/// </summary>
public record McpToolCallParams
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public Dictionary<string, object?> Arguments { get; set; } = new();
}

/// <summary>
/// MCP tool definition
/// </summary>
public record McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public McpInputSchema InputSchema { get; set; } = new();

    public Func<Dictionary<string, object?>, Task<McpToolCallResult>>? CallAsync { get; set; }
}

/// <summary>
/// MCP tool input schema (JSON Schema)
/// </summary>
public record McpInputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, McpPropertyDefinition> Properties { get; set; } = new();

    [JsonPropertyName("required")]
    public string[] Required { get; set; } = Array.Empty<string>();

    [JsonPropertyName("additionalProperties")]
    public bool AdditionalProperties { get; set; } = false;
}

/// <summary>
/// MCP property definition for input schema
/// </summary>
public record McpPropertyDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Enum { get; set; }

    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Default { get; set; }
}

/// <summary>
/// MCP tools list response
/// </summary>
public record McpToolsListResult
{
    [JsonPropertyName("tools")]
    public McpTool[] Tools { get; set; } = Array.Empty<McpTool>();
}

/// <summary>
/// MCP tool call result
/// </summary>
public record McpToolCallResult
{
    [JsonPropertyName("content")]
    public McpContent[] Content { get; set; } = Array.Empty<McpContent>();

    [JsonPropertyName("isError")]
    public bool IsError { get; set; } = false;
}

/// <summary>
/// MCP content item
/// </summary>
public record McpContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MimeType { get; set; }
}

/// <summary>
/// MCP method names
/// </summary>
public static class McpMethods
{
    public const string Initialize = "initialize";
    public const string Initialized = "notifications/initialized";
    public const string ToolsList = "tools/list";
    public const string ToolsCall = "tools/call";
}
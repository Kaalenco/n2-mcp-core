using McpCore.JsonRpc;
using McpCore.Protocol;
using McpCore.Server;

namespace N2.McpCore.Unittests;

/// <summary>
/// Minimal concrete server used only in tests.
/// Exposes one fixed tool and records the last tool call.
/// </summary>
internal class TestMcpServer : McpServer
{
    public string? LastCalledTool { get; private set; }

    public TestMcpServer() : base(
        new McpServerInfo { Name = "test-server", Version = "0.1" },
        new McpServerCapabilities { Tools = new McpToolsCapability { ListChanged = false } })
    {
    }

    protected override McpTool[] GetAvailableTools() =>
    [
        new McpTool
        {
            Name = "echo",
            CallAsync = async (args) =>
            {
                var text = (string)args["text"]!;
                LastCalledTool = "echo";
                return new McpToolCallResult
                {
                    Content = [new McpContent { Type = "text", Text = text }]
                };
            },
            Description = "Echoes its input",
            InputSchema = new McpInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, McpPropertyDefinition>
                {
                    ["text"] = new McpPropertyDefinition { Type = "string", Description = "text to echo" }
                },
                Required = ["text"]
            }
        }
    ];
}

public class UsingMcpServer
{
    private TestMcpServer _server = null!;

    [SetUp]
    public void Setup()
    {
        _server = new TestMcpServer();
    }

    // ── Initial state ────────────────────────────────────────────────────────

    [Test]
    public void NewServer_IsNotInitialized()
    {
        Assert.That(_server.Initialized, Is.False);
    }

    [Test]
    public async Task GetToolsListAsync_BeforeInitialization_ThrowsInvalidOperationException()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() => _server.GetToolsListAsync());
        await Task.CompletedTask;
    }

    [Test]
    public async Task CallToolAsync_BeforeInitialization_ThrowsInvalidOperationException()
    {
        var server = new TestMcpServer();
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            server.CallToolAsync(new McpToolCallParams { Name = "echo" }));
        await Task.CompletedTask;
    }

    // ── ProcessRequestAsync null guard ───────────────────────────────────────

    [Test]
    public async Task ProcessRequestAsync_NullRequest_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _server.ProcessRequestAsync(null!));
        await Task.CompletedTask;
    }

    // ── Unknown method ────────────────────────────────────────────────────────

    [Test]
    public async Task ProcessRequestAsync_UnknownMethod_ReturnsMethodNotFoundError()
    {
        var request = new JsonRpcRequest { Id = 1, Method = "no/such/method" };

        var response = await _server.ProcessRequestAsync(request);

        Assert.That(response.Error, Is.Not.Null);
        Assert.That(response.Error!.Code, Is.EqualTo(JsonRpcErrorCodes.MethodNotFound));
    }

    [Test]
    public async Task ProcessRequestAsync_UnknownMethod_ErrorMessageContainsMethodName()
    {
        var request = new JsonRpcRequest { Id = 1, Method = "tools/unknown" };

        var response = await _server.ProcessRequestAsync(request);

        Assert.That(response.Error!.Message, Does.Contain("tools/unknown"));
    }

    // ── initialize handshake ─────────────────────────────────────────────────

    [Test]
    public async Task ProcessRequestAsync_Initialize_ReturnsProtocolVersion()
    {
        var request = new JsonRpcRequest
        {
            Id = 1,
            Method = McpMethods.Initialize,
            Params = new McpInitializeParams()
        };

        var response = await _server.ProcessRequestAsync(request);

        Assert.That(response.Error, Is.Null);
        Assert.That(response.Result, Is.InstanceOf<McpInitializeResult>());
        var result = (McpInitializeResult)response.Result!;
        Assert.That(result.ProtocolVersion, Is.EqualTo("2024-11-05"));
    }

    [Test]
    public async Task ProcessRequestAsync_Initialize_ReturnsServerInfo()
    {
        var request = new JsonRpcRequest
        {
            Id = 1,
            Method = McpMethods.Initialize,
            Params = new McpInitializeParams()
        };

        var response = await _server.ProcessRequestAsync(request);

        var result = (McpInitializeResult)response.Result!;
        Assert.That(result.ServerInfo.Name, Is.EqualTo("test-server"));
        Assert.That(result.ServerInfo.Version, Is.EqualTo("0.1"));
    }

    [Test]
    public async Task ProcessRequestAsync_Initialize_DoesNotSetInitializedYet()
    {
        var request = new JsonRpcRequest
        {
            Id = 1,
            Method = McpMethods.Initialize,
            Params = new McpInitializeParams()
        };

        await _server.ProcessRequestAsync(request);

        Assert.That(_server.Initialized, Is.False);
    }

    // ── initialized notification ──────────────────────────────────────────────

    [Test]
    public async Task ProcessRequestAsync_InitializedNotification_SetsServerInitialized()
    {
        // Send initialize first (required by protocol, though the server doesn't enforce order)
        await _server.ProcessRequestAsync(new JsonRpcRequest
        {
            Id = 1,
            Method = McpMethods.Initialize,
            Params = new McpInitializeParams()
        });

        // The initialized notification has no Id (it's a notification)
        await _server.ProcessRequestAsync(new JsonRpcRequest
        {
            Id = null,
            Method = McpMethods.Initialized
        });

        Assert.That(_server.Initialized, Is.True);
    }

    // ── Full handshake → tools/list ──────────────────────────────────────────

    [Test]
    public async Task FullHandshake_ThenToolsList_ReturnsRegisteredTools()
    {
        await DoFullHandshakeAsync();

        var request = new JsonRpcRequest { Id = 3, Method = McpMethods.ToolsList };
        var response = await _server.ProcessRequestAsync(request);

        Assert.That(response.Error, Is.Null);
        Assert.That(response.Result, Is.InstanceOf<McpToolsListResult>());
        var list = (McpToolsListResult)response.Result!;
        Assert.That(list.Tools, Has.Length.EqualTo(1));
        Assert.That(list.Tools[0].Name, Is.EqualTo("echo"));
    }

    // ── Full handshake → tools/call ───────────────────────────────────────────

    [Test]
    public async Task FullHandshake_ThenToolsCall_InvokesCallToolAsync()
    {
        await DoFullHandshakeAsync();

        var request = new JsonRpcRequest
        {
            Id = 4,
            Method = McpMethods.ToolsCall,
            Params = new McpToolCallParams { Name = "echo", Arguments = new Dictionary<string, object?> { ["text"] = "hi" } }
        };

        var response = await _server.ProcessRequestAsync(request);

        Assert.That(response.Error, Is.Null);
        Assert.That(_server.LastCalledTool, Is.EqualTo("echo"));
    }

    // ── ArgumentException → InvalidParams ────────────────────────────────────

    [Test]
    public async Task ProcessRequestAsync_HandlerThrowsArgumentException_ReturnsInvalidParamsError()
    {
        // Pass a bad Params type that cannot be deserialized to McpInitializeParams
        // (an integer is not a valid McpInitializeParams)
        var request = new JsonRpcRequest
        {
            Id = 5,
            Method = McpMethods.Initialize,
            Params = 12345   // wrong type — triggers ArgumentException in DeserializeParams
        };

        var response = await _server.ProcessRequestAsync(request);

        Assert.That(response.Error, Is.Not.Null);
        Assert.That(response.Error!.Code, Is.EqualTo(JsonRpcErrorCodes.InvalidParams));
    }

    // ── Response id mirrors request id ────────────────────────────────────────

    [Test]
    public async Task ProcessRequestAsync_ResponseId_MatchesRequestId()
    {
        var request = new JsonRpcRequest { Id = 99, Method = "no/method" };

        var response = await _server.ProcessRequestAsync(request);

        Assert.That(response.Id, Is.EqualTo(99));
    }

    // ── InitializeAsync directly ──────────────────────────────────────────────

    [Test]
    public async Task InitializeAsync_WithClientInfo_ReturnsResult()
    {
        var parameters = new McpInitializeParams
        {
            ProtocolVersion = "2024-11-05",
            ClientInfo = new McpClientInfo { Name = "test-client", Version = "1.0" }
        };

        var result = await _server.InitializeAsync(parameters);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProtocolVersion, Is.EqualTo("2024-11-05"));
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private async Task DoFullHandshakeAsync()
    {
        await _server.ProcessRequestAsync(new JsonRpcRequest
        {
            Id = 1,
            Method = McpMethods.Initialize,
            Params = new McpInitializeParams()
        });

        await _server.ProcessRequestAsync(new JsonRpcRequest
        {
            Id = null,
            Method = McpMethods.Initialized
        });
    }
}

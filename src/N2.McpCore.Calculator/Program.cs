using Microsoft.Extensions.DependencyInjection;

using N2.McpCore.Calculator;
using N2.McpCore.Server;

// Create log file
var logFileName = $"calculator-mcp-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log";
var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), logFileName);
var logWriter = new StreamWriter(logFilePath, append: true) { AutoFlush = true };

// Log startup info
var startupMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Calculator MCP Server starting... Log file: {logFilePath}";
Console.Error.WriteLine(startupMessage);
logWriter.WriteLine(startupMessage);

// Configure services
var services = new ServiceCollection();
services.AddSingleton<CalculatorServer>();
var serviceProvider = services.BuildServiceProvider();

// Run the MCP server using StdIO
var exitCode = await serviceProvider.RunMcpServerAsync<CalculatorServer>(
    Console.In,
    Console.Out,
    (message) =>
    {
        Console.Error.WriteLine(message);
        logWriter.WriteLine(message);
        logWriter.Flush();
    }
);

// Cleanup
logWriter.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Server shutting down with exit code: {exitCode}");
logWriter.Close();

return exitCode;

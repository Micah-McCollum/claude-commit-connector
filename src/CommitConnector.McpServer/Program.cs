using System;
using CommitConnector.Core;
using CommitConnector.Core.Configuration;
using CommitConnector.Core.Ranger;
using CommitConnector.McpServer.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Claude starts this exe from an arbitrary working directory, so load
// appsettings.json from next to the exe rather than the current directory.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

// stdout carries the MCP protocol; all logging must go to stderr.
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

var rangerOptions = builder.Configuration.GetSection(RangerMspOptions.SectionName).Get<RangerMspOptions>()
    ?? new RangerMspOptions();
rangerOptions.Validate();

builder.Services.AddSingleton(rangerOptions);
builder.Services.AddSingleton<RangerSession>();
builder.Services.AddSingleton<ICommitRepository, RangerCommitRepository>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<AccountTools>()
    .WithTools<TicketTools>();

await builder.Build().RunAsync();

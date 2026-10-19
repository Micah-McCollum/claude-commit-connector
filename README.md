# claude-commit-connector

Claude ↔ Commit (RangerMSP) connector over the RangerMSP Web API, written in C#.

The connector is an [MCP](https://modelcontextprotocol.io) server. Claude
starts it as a local process, and it talks to RangerMSP through the official
RangerMSP C# API library (`CRMLib`) in Web API mode.

```
Claude Desktop / Claude Code
   │  MCP over stdio
   ▼
CommitConnector.McpServer.exe   (.NET Framework 4.8, Windows)
   │  CRMLib, UseWebAPI = true
   ▼
RangerMSP Web API  :4964        (CRM Server service, DbQry module)
   ▼
RangerMSP database
```

The first version is **read-only**: it only needs the DbQry module and password.

## Tools exposed to Claude

| Tool | Description |
|---|---|
| `find_accounts` | Search accounts by text in the account file name |
| `get_account` | Get an account by `AccountREC_ID` |
| `find_tickets_for_account` | List tickets for an account |
| `get_ticket` | Get a ticket by `TicketREC_ID` |

## Repository layout

```
CommitConnector.sln
Directory.Build.props              net48 / x86 / CRMLib.dll location for all projects
config/
  claude_desktop_config.example.json   Claude Desktop registration
  mcp.example.json                     Claude Code (.mcp.json) registration
docs/
  server-configuration.md          RangerMSP server + credential setup
  environment-variable-setup.md    step-by-step: password in RANGERMSP_WEBAPI_PASSWORD
lib/RangerMSP/                     put CRMLib.dll here (git-ignored)
src/
  CommitConnector.Core/            everything that touches RangerMSP
    Configuration/RangerMspOptions.cs   settings from appsettings.json
    Security/                      Credential Manager / env var password lookup
    Models/                        DTOs returned to Claude
    Ranger/RangerSession.cs        CRM.Application Initialize/Terminate (wiki sample)
    Ranger/RangerCommitRepository.cs    ObjectQuery / AddCriteria / FetchObjects (wiki sample)
    ICommitRepository.cs           interface the tools depend on
  CommitConnector.McpServer/       MCP server exe
    Program.cs                     host, DI, stdio transport
    Tools/                         the MCP tools Claude sees
    appsettings.json               Web API URL and limits (no password)
samples/
  CommitConnector.ConnectionTest/  the wiki's C# sample adapted to the Web API
```

## Setup

### Prerequisites

- Windows machine that can reach the RangerMSP Web API
- Visual Studio 2022 or the .NET SDK (8+) with the .NET Framework 4.8 targeting pack
- RangerMSP 7.0+ API download (for `CRMLib`)

### Steps

1. **Server:** follow [docs/server-configuration.md](docs/server-configuration.md):
   give DbQry its own password, turn DbEng off and store the DbQry password in
   Credential Manager as `RangerMSP-DbQry`.
2. **CRMLib:** build it and copy `CRMLib.dll` into `lib/RangerMSP/`
   (see [lib/RangerMSP/README.md](lib/RangerMSP/README.md)).
3. **Build:**
   ```powershell
   dotnet build CommitConnector.sln -c Release
   ```
   If a CRMLib member name doesn't compile, fix it in
   `src/CommitConnector.Core/Ranger/RangerCommitRepository.cs` (see the VERIFY
   note at the top of that file).
4. **Test the connection** (no Claude involved):
   ```powershell
   samples\CommitConnector.ConnectionTest\bin\Release\net48\CommitConnector.ConnectionTest.exe http://your-server:4964/ "New York"
   ```
5. **Configure:** set `RangerMsp:WebApiUrl` in
   `src/CommitConnector.McpServer/appsettings.json`, then copy
   `src/CommitConnector.McpServer/bin/Release/net48/` to e.g. `C:\Tools\CommitConnector\`.
6. **Register with Claude:**
   - Claude Desktop: merge `config/claude_desktop_config.example.json` into
     `%APPDATA%\Claude\claude_desktop_config.json` and restart Claude Desktop.
   - Claude Code: copy `config/mcp.example.json` to `.mcp.json` in your project,
     or run `claude mcp add rangermsp -- C:\Tools\CommitConnector\CommitConnector.McpServer.exe`.
7. Ask Claude something like *"Find the RangerMSP account for Contoso and list its tickets."*

## Security notes

- The password is never in a config file: it comes from Windows Credential
  Manager (or `RANGERMSP_WEBAPI_PASSWORD` for dev/test).
- Read-only by design. Write tools (create ticket, add note) will need DbEng
  and should be added deliberately.
- Results are capped (`MaxResultsLimit`, default 100) to avoid Web API
  timeouts on large fetches.

## References

- [API Developers Guide](https://www.rangermsp.com/wiki/API_Developers_Guide)
- [CSharp API](https://www.rangermsp.com/wiki/CSharp_API)
- [CSharp API Library Setup](https://www.rangermsp.com/wiki/CSharp_API_Library_Setup)
- [Web API Server Configuration](https://www.rangermsp.com/wiki/Web_API_Server_Configuration)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)

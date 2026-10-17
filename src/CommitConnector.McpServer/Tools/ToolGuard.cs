using System;
using System.Threading.Tasks;
using CommitConnector.Core;
using ModelContextProtocol;

namespace CommitConnector.McpServer.Tools
{
    /// <summary>
    /// The MCP SDK hides the message of ordinary exceptions from the client.
    /// Connector errors are written to be safe and useful (e.g. "no password
    /// found", RangerMSP API errors), so pass them through as McpException.
    /// </summary>
    internal static class ToolGuard
    {
        public static async Task<T> RunAsync<T>(Func<Task<T>> call)
        {
            try
            {
                return await call().ConfigureAwait(false);
            }
            catch (CommitConnectorException ex)
            {
                throw new McpException(ex.Message, ex);
            }
        }
    }
}

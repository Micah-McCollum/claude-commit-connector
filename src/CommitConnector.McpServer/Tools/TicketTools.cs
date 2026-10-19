using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommitConnector.Core;
using CommitConnector.Core.Configuration;
using CommitConnector.Core.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace CommitConnector.McpServer.Tools
{
    [McpServerToolType]
    public sealed class TicketTools
    {
        private readonly ICommitRepository _repository;
        private readonly RangerMspOptions _options;

        public TicketTools(ICommitRepository repository, RangerMspOptions options)
        {
            _repository = repository;
            _options = options;
        }

        [McpServerTool(Name = "find_tickets_for_account", ReadOnly = true), Description(
            "List RangerMSP (Commit) tickets belonging to an account.")]
        public async Task<IReadOnlyList<TicketSummary>> FindTicketsForAccount(
            [Description("The account's AccountREC_ID, as returned by find_accounts.")] string accountRecId,
            [Description("Maximum number of tickets to return. Defaults to the server setting.")] int? maxResults = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountRecId))
                throw new McpException("accountRecId is required.");

            return await ToolGuard.RunAsync(() =>
                _repository.FindTicketsForAccountAsync(accountRecId.Trim(), _options.ClampMaxResults(maxResults), cancellationToken));
        }

        [McpServerTool(Name = "get_ticket", ReadOnly = true), Description(
            "Get one RangerMSP (Commit) ticket by its ticket record ID (TicketREC_ID).")]
        public async Task<TicketSummary> GetTicket(
            [Description("The ticket's TicketREC_ID, as returned by find_tickets_for_account.")] string ticketRecId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ticketRecId))
                throw new McpException("ticketRecId is required.");

            var ticket = await ToolGuard.RunAsync(() => _repository.GetTicketAsync(ticketRecId.Trim(), cancellationToken));
            return ticket ?? throw new McpException($"No ticket found with ID '{ticketRecId}'.");
        }
    }
}

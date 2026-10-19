using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommitConnector.Core.Models;

namespace CommitConnector.Core
{
    /// <summary>
    /// Read-only access to RangerMSP. The MCP tools depend on this interface
    /// rather than on CRMLib, so they can be tested without a server.
    /// </summary>
    public interface ICommitRepository
    {
        Task<IReadOnlyList<AccountSummary>> FindAccountsAsync(string nameContains, int maxResults, CancellationToken cancellationToken);

        Task<AccountSummary?> GetAccountAsync(string accountRecId, CancellationToken cancellationToken);

        Task<IReadOnlyList<TicketSummary>> FindTicketsForAccountAsync(string accountRecId, int maxResults, CancellationToken cancellationToken);

        Task<TicketSummary?> GetTicketAsync(string ticketRecId, CancellationToken cancellationToken);
    }
}

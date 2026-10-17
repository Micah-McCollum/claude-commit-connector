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
    public sealed class AccountTools
    {
        private readonly ICommitRepository _repository;
        private readonly RangerMspOptions _options;

        public AccountTools(ICommitRepository repository, RangerMspOptions options)
        {
            _repository = repository;
            _options = options;
        }

        [McpServerTool(Name = "find_accounts", ReadOnly = true), Description(
            "Search RangerMSP (Commit) accounts whose file name contains the given text. " +
            "Returns account IDs to use with get_account and find_tickets_for_account.")]
        public async Task<IReadOnlyList<AccountSummary>> FindAccounts(
            [Description("Text to look for in the account's file name, e.g. part of the company name.")] string name,
            [Description("Maximum number of accounts to return. Defaults to the server setting.")] int? maxResults = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new McpException("Provide some text to search for.");

            return await ToolGuard.RunAsync(() =>
                _repository.FindAccountsAsync(name, _options.ClampMaxResults(maxResults), cancellationToken));
        }

        [McpServerTool(Name = "get_account", ReadOnly = true), Description(
            "Get one RangerMSP (Commit) account by its account record ID (AccountREC_ID).")]
        public async Task<AccountSummary> GetAccount(
            [Description("The account's AccountREC_ID, as returned by find_accounts.")] string accountRecId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountRecId))
                throw new McpException("accountRecId is required.");

            var account = await ToolGuard.RunAsync(() => _repository.GetAccountAsync(accountRecId.Trim(), cancellationToken));
            return account ?? throw new McpException($"No account found with ID '{accountRecId}'.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommitConnector.Core.Models;

namespace CommitConnector.Core.Ranger
{
    /// <summary>
    /// <see cref="ICommitRepository"/> built on the RangerMSP C# API. Queries
    /// follow the wiki's ObjectQuery / AddCriteria / FetchObjects sample.
    ///
    /// VERIFY AGAINST YOUR CRMLib BUILD: the wiki confirms CRM.Account.Fields.City,
    /// CRM.OperatorEnum.opEqual, AccountREC_ID and Ticket.Description. The other
    /// property, field and operator names below follow the RangerMSP API Reference
    /// Manual naming and may differ slightly in your version. If one doesn't
    /// compile, fix the name here; nothing outside this file uses CRMLib except
    /// RangerSession.
    /// </summary>
    public sealed class RangerCommitRepository : ICommitRepository
    {
        private readonly RangerSession _session;

        public RangerCommitRepository(RangerSession session)
        {
            _session = session;
        }

        public Task<IReadOnlyList<AccountSummary>> FindAccountsAsync(string nameContains, int maxResults, CancellationToken cancellationToken) =>
            _session.RunAsync<IReadOnlyList<AccountSummary>>(() =>
            {
                CRM.ObjectQuery<CRM.Account> accountSearch = new CRM.ObjectQuery<CRM.Account>();
                accountSearch.AddCriteria(CRM.Account.Fields.FileName, CRM.OperatorEnum.opLike, "%" + StripWildcards(nameContains) + "%");
                List<CRM.Account> accounts = accountSearch.FetchObjects();
                return accounts.Take(maxResults).Select(ToSummary).ToList();
            }, cancellationToken);

        public Task<AccountSummary?> GetAccountAsync(string accountRecId, CancellationToken cancellationToken) =>
            _session.RunAsync<AccountSummary?>(() =>
            {
                CRM.ObjectQuery<CRM.Account> accountSearch = new CRM.ObjectQuery<CRM.Account>();
                accountSearch.AddCriteria(CRM.Account.Fields.AccountREC_ID, CRM.OperatorEnum.opEqual, accountRecId);
                CRM.Account? account = accountSearch.FetchObjects().FirstOrDefault();
                return account == null ? null : ToSummary(account);
            }, cancellationToken);

        public Task<IReadOnlyList<TicketSummary>> FindTicketsForAccountAsync(string accountRecId, int maxResults, CancellationToken cancellationToken) =>
            _session.RunAsync<IReadOnlyList<TicketSummary>>(() =>
            {
                CRM.ObjectQuery<CRM.Ticket> ticketSearch = new CRM.ObjectQuery<CRM.Ticket>();
                ticketSearch.AddCriteria(CRM.Ticket.Fields.AccountREC_ID, CRM.OperatorEnum.opEqual, accountRecId);
                List<CRM.Ticket> tickets = ticketSearch.FetchObjects();
                return tickets.Take(maxResults).Select(ToSummary).ToList();
            }, cancellationToken);

        public Task<TicketSummary?> GetTicketAsync(string ticketRecId, CancellationToken cancellationToken) =>
            _session.RunAsync<TicketSummary?>(() =>
            {
                CRM.ObjectQuery<CRM.Ticket> ticketSearch = new CRM.ObjectQuery<CRM.Ticket>();
                ticketSearch.AddCriteria(CRM.Ticket.Fields.TicketREC_ID, CRM.OperatorEnum.opEqual, ticketRecId);
                CRM.Ticket? ticket = ticketSearch.FetchObjects().FirstOrDefault();
                return ticket == null ? null : ToSummary(ticket);
            }, cancellationToken);

        private static AccountSummary ToSummary(CRM.Account account) => new AccountSummary
        {
            AccountRecId = Text(account.AccountREC_ID),
            FileName = Text(account.FileName),
            CompanyName = Text(account.CompanyName),
            Phone = Text(account.Phone1),
            Email = Text(account.EmailAddress1),
            City = Text(account.City),
            State = Text(account.State),
        };

        private static TicketSummary ToSummary(CRM.Ticket ticket) => new TicketSummary
        {
            TicketRecId = Text(ticket.TicketREC_ID),
            TicketNumber = Text(ticket.TicketNumber),
            AccountRecId = Text(ticket.AccountREC_ID),
            Description = Text(ticket.Description),
            Status = Text(ticket.Status),
            Priority = Text(ticket.Priority),
        };

        // Field types (string, int, enum, DateTime) vary between CRMLib versions;
        // this keeps the mapping compiling whichever one a property uses.
        private static string Text(object? value) =>
            Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";

        // Keep user input a literal substring, not a LIKE pattern.
        private static string StripWildcards(string value) =>
            value.Replace("%", "").Replace("_", "").Trim();
    }
}

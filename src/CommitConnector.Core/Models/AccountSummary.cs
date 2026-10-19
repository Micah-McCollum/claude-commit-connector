namespace CommitConnector.Core.Models
{
    /// <summary>The account fields returned to Claude. Kept small on purpose.</summary>
    public sealed class AccountSummary
    {
        public string AccountRecId { get; set; } = "";
        public string FileName { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
    }
}

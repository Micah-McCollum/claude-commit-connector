namespace CommitConnector.Core.Models
{
    /// <summary>The ticket fields returned to Claude. Kept small on purpose.</summary>
    public sealed class TicketSummary
    {
        public string TicketRecId { get; set; } = "";
        public string TicketNumber { get; set; } = "";
        public string AccountRecId { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
    }
}

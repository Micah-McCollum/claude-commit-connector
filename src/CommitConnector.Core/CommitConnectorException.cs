using System;

namespace CommitConnector.Core
{
    /// <summary>
    /// A failure talking to RangerMSP whose message is safe to show to Claude
    /// (it never includes the password).
    /// </summary>
    public sealed class CommitConnectorException : Exception
    {
        public CommitConnectorException(string message) : base(message) { }

        public CommitConnectorException(string message, Exception innerException) : base(message, innerException) { }
    }
}

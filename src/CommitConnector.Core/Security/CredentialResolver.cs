using System;
using CommitConnector.Core.Configuration;

namespace CommitConnector.Core.Security
{
    /// <summary>
    /// Finds the Web API module password without it ever living in a config
    /// file: Windows Credential Manager first, then an environment variable.
    /// </summary>
    public static class CredentialResolver
    {
        public static string ResolvePassword(RangerMspOptions options)
        {
            var secret = WindowsCredentialStore.ReadSecret(options.CredentialTarget);
            if (!string.IsNullOrEmpty(secret))
                return secret!;

            secret = Environment.GetEnvironmentVariable(options.PasswordEnvironmentVariable);
            if (!string.IsNullOrEmpty(secret))
                return secret!;

            throw new CommitConnectorException(
                $"No RangerMSP Web API password found. Add a generic credential named '{options.CredentialTarget}' " +
                $"in Windows Credential Manager for the account running the connector, or set the " +
                $"'{options.PasswordEnvironmentVariable}' environment variable.");
        }
    }
}

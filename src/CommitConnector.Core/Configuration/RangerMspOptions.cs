using System;

namespace CommitConnector.Core.Configuration
{
    /// <summary>
    /// Connection settings for the RangerMSP Web API, bound from the "RangerMsp"
    /// section of appsettings.json. The password is deliberately not a setting;
    /// see <see cref="Security.CredentialResolver"/>.
    /// </summary>
    public sealed class RangerMspOptions
    {
        public const string SectionName = "RangerMsp";

        /// <summary>Recorded by RangerMSP as the application that made each change.</summary>
        public string AppName { get; set; } = "Claude Commit Connector";

        /// <summary>
        /// Web API address, including scheme, e.g. http://localhost:4964/ or
        /// https://webapi.example.com/. The connection fails without the scheme.
        /// </summary>
        public string WebApiUrl { get; set; } = "http://localhost:4964/";

        /// <summary>
        /// The RangerMSP\ThirdParty\UserDev folder holding the RangerMSP API DLLs.
        /// CRMLib requires it even in Web API mode ("DLL folder must be set").
        /// From another machine, use a UNC path to a read-only share of that
        /// folder, e.g. \\RANGERSERVER\RangerUserDev (not a mapped drive letter).
        /// </summary>
        public string? DllFolder { get; set; }

        /// <summary>
        /// Optional: the RangerMSP\Db folder. Not used in Web API mode; set it only
        /// if your CRMLib build refuses to initialize without it.
        /// </summary>
        public string? DbFolder { get; set; }

        /// <summary>Windows Credential Manager generic credential holding the DbQry password.</summary>
        public string CredentialTarget { get; set; } = "RangerMSP-DbQry";

        /// <summary>Fallback environment variable for the password, for dev/test machines.</summary>
        public string PasswordEnvironmentVariable { get; set; } = "RANGERMSP_WEBAPI_PASSWORD";

        /// <summary>Default number of rows returned to Claude by search tools.</summary>
        public int DefaultMaxResults { get; set; } = 25;

        /// <summary>Hard upper limit on rows returned, to avoid Web API timeouts on large fetches.</summary>
        public int MaxResultsLimit { get; set; } = 100;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(AppName))
                throw new InvalidOperationException($"{SectionName}:AppName is required.");

            if (!Uri.TryCreate(WebApiUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException(
                    $"{SectionName}:WebApiUrl must be an absolute http:// or https:// URL (got '{WebApiUrl}').");

            if (!WebApiUrl.EndsWith("/", StringComparison.Ordinal))
                WebApiUrl += "/";

            if (DefaultMaxResults < 1 || MaxResultsLimit < DefaultMaxResults)
                throw new InvalidOperationException(
                    $"{SectionName}: require 1 <= DefaultMaxResults <= MaxResultsLimit.");
        }

        public int ClampMaxResults(int? requested) =>
            Math.Max(1, Math.Min(requested ?? DefaultMaxResults, MaxResultsLimit));
    }
}

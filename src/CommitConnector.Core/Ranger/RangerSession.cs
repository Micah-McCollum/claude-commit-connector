using System;
using System.Threading;
using System.Threading.Tasks;
using CommitConnector.Core.Configuration;
using CommitConnector.Core.Security;
using Microsoft.Extensions.Logging;

namespace CommitConnector.Core.Ranger
{
    /// <summary>
    /// Owns the process-wide CRMLib connection. CRM.Application is static and
    /// not documented as thread-safe, so every call is serialized through one
    /// gate. The connection opens on first use, so the MCP server still starts
    /// (and reports a clear error) when RangerMSP is unreachable.
    /// </summary>
    public sealed class RangerSession : IDisposable
    {
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly RangerMspOptions _options;
        private readonly ILogger<RangerSession> _logger;
        private bool _initialized;

        public RangerSession(RangerMspOptions options, ILogger<RangerSession> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<T> RunAsync<T>(Func<T> work, CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await Task.Run(() =>
                {
                    EnsureInitialized();
                    return work();
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (CRM.Exception ex)
            {
                _logger.LogError(ex, "RangerMSP API call failed");
                throw new CommitConnectorException("RangerMSP API error: " + ex.Message, ex);
            }
            finally
            {
                _gate.Release();
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            // From the RangerMSP wiki, "CSharp API" > "Remote Server (Web API)".
            CRM.Config config = new CRM.Config();
            config.AppName = _options.AppName;
            config.WebAPIUrl = _options.WebApiUrl;
            config.Password = CredentialResolver.ResolvePassword(_options);
            config.UseWebAPI = true;
            config.DllFolder = RangerFolders.RequireDllFolder(_options.DllFolder);
            if (!string.IsNullOrWhiteSpace(_options.DbFolder))
                config.DbFolder = _options.DbFolder;

            _logger.LogInformation("Connecting to RangerMSP Web API at {WebApiUrl}", _options.WebApiUrl);
            CRM.Application.Initialize(config);
            _initialized = true;
        }

        public void Dispose()
        {
            if (_initialized)
            {
                try
                {
                    CRM.Application.Terminate();
                }
                catch (CRM.Exception ex)
                {
                    _logger.LogWarning(ex, "RangerMSP Terminate failed");
                }
                _initialized = false;
            }
            _gate.Dispose();
        }
    }
}

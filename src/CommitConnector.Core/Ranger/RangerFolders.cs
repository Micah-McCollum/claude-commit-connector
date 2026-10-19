using System.IO;

namespace CommitConnector.Core.Ranger
{
    /// <summary>
    /// Checks the folder settings up front, so a wrong path or a share the
    /// account can't open gives a clear message instead of a CRMLib error.
    /// </summary>
    public static class RangerFolders
    {
        public static string RequireDllFolder(string? dllFolder)
        {
            if (string.IsNullOrWhiteSpace(dllFolder))
                throw new CommitConnectorException(
                    "RangerMsp:DllFolder is not set. CRMLib needs the RangerMSP\\ThirdParty\\UserDev folder even in " +
                    "Web API mode. On the RangerMSP server use e.g. C:\\RangerMSP\\ThirdParty\\UserDev; from another " +
                    "machine use a UNC path to a read-only share of it, e.g. \\\\RANGERSERVER\\RangerUserDev.");

            var folder = dllFolder!.Trim().TrimEnd('\\', '/');

            // Directory.Exists also returns false when a UNC share exists but this
            // Windows account has no permission on it.
            if (!Directory.Exists(folder))
                throw new CommitConnectorException(
                    $"RangerMsp:DllFolder '{folder}' can't be opened. Check the path, that it is a UNC path " +
                    "(\\\\server\\share) rather than a mapped drive letter, and that the Windows account running " +
                    "the connector has read access to the share.");

            return folder;
        }
    }
}

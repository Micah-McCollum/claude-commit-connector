using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace CommitConnector.Core.Security
{
    /// <summary>
    /// Reads generic credentials from Windows Credential Manager for the current
    /// Windows user (Control Panel > Credential Manager > Windows Credentials >
    /// Add a generic credential).
    /// </summary>
    public static class WindowsCredentialStore
    {
        private const int CRED_TYPE_GENERIC = 1;
        private const int ERROR_NOT_FOUND = 1168;

        /// <summary>Returns the stored secret, or null when no credential has that target name.</summary>
        public static string? ReadSecret(string targetName)
        {
            if (!CredRead(targetName, CRED_TYPE_GENERIC, 0, out var credentialPtr))
            {
                var error = Marshal.GetLastWin32Error();
                if (error == ERROR_NOT_FOUND)
                    return null;
                throw new Win32Exception(error, $"Reading credential '{targetName}' failed.");
            }

            try
            {
                var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
                if (credential.CredentialBlob == IntPtr.Zero || credential.CredentialBlobSize == 0)
                    return null;

                // Credentials added through Control Panel are stored as UTF-16.
                return Marshal.PtrToStringUni(credential.CredentialBlob, credential.CredentialBlobSize / 2);
            }
            finally
            {
                CredFree(credentialPtr);
            }
        }

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("advapi32.dll")]
        private static extern void CredFree(IntPtr buffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public int Flags;
            public int Type;
            public string TargetName;
            public string Comment;
            public FILETIME LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }
    }
}

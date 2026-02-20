namespace Phase3.AzureDevOps.Services.Secrets;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Core;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Provides secrets storage using Windows Credential Manager.
/// </summary>
public class WindowsCredentialManagerSecretsProvider : ISecretsProvider
{
    private readonly ILogger<WindowsCredentialManagerSecretsProvider> _logger;
    private const string TargetPrefix = "AutonomousAgent:";

    public string ProviderName => "WindowsCredentialManager";

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsCredentialManagerSecretsProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public WindowsCredentialManagerSecretsProvider(ILogger<WindowsCredentialManagerSecretsProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("Windows Credential Manager is only supported on Windows.");

        _logger.LogInformation("WindowsCredentialManagerSecretsProvider initialized");
    }

    /// <summary>
    /// Gets a secret from Windows Credential Manager.
    /// </summary>
    public Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        _logger.LogDebug("Retrieving secret {SecretName} from Credential Manager", secretName);

        var targetName = GetTargetName(secretName);

        if (!CredRead(targetName, CRED_TYPE_GENERIC, 0, out var credentialPtr))
        {
            var error = Marshal.GetLastWin32Error();
            if (error == ERROR_NOT_FOUND)
            {
                _logger.LogWarning("Secret {SecretName} not found in Credential Manager", secretName);
                throw new SecretNotFoundException(secretName);
            }

            throw new InvalidOperationException($"Failed to read credential: Win32 error {error}");
        }

        try
        {
            var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
            var password = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);

            _logger.LogDebug("Secret {SecretName} retrieved successfully", secretName);
            return Task.FromResult(password ?? string.Empty);
        }
        finally
        {
            CredFree(credentialPtr);
        }
    }

    /// <summary>
    /// Sets a secret in Windows Credential Manager.
    /// </summary>
    public Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        if (string.IsNullOrEmpty(secretValue))
            throw new ArgumentException("Secret value cannot be null or empty.", nameof(secretValue));

        _logger.LogInformation("Setting secret {SecretName} in Credential Manager", secretName);

        var targetName = GetTargetName(secretName);
        var passwordBytes = Encoding.Unicode.GetBytes(secretValue);

        var credential = new CREDENTIAL
        {
            Type = CRED_TYPE_GENERIC,
            TargetName = targetName,
            CredentialBlob = Marshal.StringToCoTaskMemUni(secretValue),
            CredentialBlobSize = (uint)passwordBytes.Length,
            Persist = CRED_PERSIST_LOCAL_MACHINE,
            UserName = Environment.UserName
        };

        try
        {
            if (!CredWrite(ref credential, 0))
            {
                var error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to write credential: Win32 error {error}");
            }

            _logger.LogInformation("Secret {SecretName} set successfully", secretName);
        }
        finally
        {
            Marshal.FreeCoTaskMem(credential.CredentialBlob);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a secret from Windows Credential Manager.
    /// </summary>
    public Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        _logger.LogInformation("Deleting secret {SecretName} from Credential Manager", secretName);

        var targetName = GetTargetName(secretName);

        if (!CredDelete(targetName, CRED_TYPE_GENERIC, 0))
        {
            var error = Marshal.GetLastWin32Error();
            if (error == ERROR_NOT_FOUND)
            {
                _logger.LogWarning("Secret {SecretName} not found in Credential Manager", secretName);
                throw new SecretNotFoundException(secretName);
            }

            throw new InvalidOperationException($"Failed to delete credential: Win32 error {error}");
        }

        _logger.LogInformation("Secret {SecretName} deleted successfully", secretName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a secret exists in Windows Credential Manager.
    /// </summary>
    public Task<bool> SecretExistsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        var targetName = GetTargetName(secretName);

        if (CredRead(targetName, CRED_TYPE_GENERIC, 0, out var credentialPtr))
        {
            CredFree(credentialPtr);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private string GetTargetName(string secretName) => $"{TargetPrefix}{secretName}";

    private void ValidateSecretName(string secretName)
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty.", nameof(secretName));
    }

    // P/Invoke declarations
    private const int CRED_TYPE_GENERIC = 1;
    private const int CRED_PERSIST_LOCAL_MACHINE = 2;
    private const int ERROR_NOT_FOUND = 1168;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, int type, int flags, out IntPtr credential);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref CREDENTIAL credential, uint flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, int type, int flags);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern void CredFree(IntPtr credential);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public int Flags;
        public int Type;
        public string TargetName;
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }
}

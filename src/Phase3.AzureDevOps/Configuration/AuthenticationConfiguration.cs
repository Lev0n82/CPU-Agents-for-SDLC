namespace Phase3.AzureDevOps.Configuration;

using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Configuration for certificate-based authentication.
/// </summary>
public class CertificateAuthenticationConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateAuthenticationConfiguration"/> class.
    /// </summary>
    public CertificateAuthenticationConfiguration() { }
    /// <summary>
    /// Gets or sets the Azure AD tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure AD client ID (application ID).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the certificate thumbprint.
    /// </summary>
    public string Thumbprint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the certificate store name.
    /// </summary>
    public StoreName StoreName { get; set; } = StoreName.My;

    /// <summary>
    /// Gets or sets the certificate store location.
    /// </summary>
    public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;
}

/// <summary>
/// Configuration for MSAL device code authentication.
/// </summary>
public class MSALDeviceAuthenticationConfiguration
{
    /// <summary>
    /// Gets or sets the scopes for authentication.
    /// </summary>
    public string[] Scopes { get; set; } = new[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" };

    /// <summary>
    /// Initializes a new instance of the <see cref="MSALDeviceAuthenticationConfiguration"/> class.
    /// </summary>
    public MSALDeviceAuthenticationConfiguration() { }
    /// <summary>
    /// Gets or sets the Azure AD tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure AD client ID (application ID).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device code callback action.
    /// </summary>
    public Action<string>? DeviceCodeCallback { get; set; }
}

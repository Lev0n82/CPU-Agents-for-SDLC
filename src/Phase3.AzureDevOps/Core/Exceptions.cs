namespace Phase3.AzureDevOps.Core;

/// <summary>
/// Exception thrown when a work item is not found.
/// </summary>
public class WorkItemNotFoundException : Exception
{
    public int WorkItemId { get; }

    public WorkItemNotFoundException(int workItemId)
        : base($"Work item {workItemId} was not found.")
    {
        WorkItemId = workItemId;
    }

    public WorkItemNotFoundException(int workItemId, string message)
        : base(message)
    {
        WorkItemId = workItemId;
    }

    public WorkItemNotFoundException(int workItemId, string message, Exception innerException)
        : base(message, innerException)
    {
        WorkItemId = workItemId;
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict is detected (ETag mismatch).
/// </summary>
public class ConcurrencyException : Exception
{
    public int WorkItemId { get; }
    public int ExpectedRevision { get; }
    public int ActualRevision { get; }

    public ConcurrencyException(int workItemId, int expectedRevision, int actualRevision)
        : base($"Concurrency conflict on work item {workItemId}: expected revision {expectedRevision}, but actual revision is {actualRevision}.")
    {
        WorkItemId = workItemId;
        ExpectedRevision = expectedRevision;
        ActualRevision = actualRevision;
    }

    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when authentication fails.
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message)
        : base(message)
    {
    }

    public AuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a certificate is not found.
/// </summary>
public class CertificateNotFoundException : Exception
{
    public string Thumbprint { get; }

    public CertificateNotFoundException(string thumbprint)
        : base($"Certificate with thumbprint {thumbprint} was not found.")
    {
        Thumbprint = thumbprint;
    }

    public CertificateNotFoundException(string thumbprint, string message)
        : base(message)
    {
        Thumbprint = thumbprint;
    }

    public CertificateNotFoundException(string thumbprint, string message, Exception innerException)
        : base(message, innerException)
    {
        Thumbprint = thumbprint;
    }
}

/// <summary>
/// Exception thrown when a secret is not found.
/// </summary>
public class SecretNotFoundException : Exception
{
    public string SecretName { get; }

    public SecretNotFoundException(string secretName)
        : base($"Secret '{secretName}' was not found.")
    {
        SecretName = secretName;
    }

    public SecretNotFoundException(string secretName, string message)
        : base(message)
    {
        SecretName = secretName;
    }

    public SecretNotFoundException(string secretName, string message, Exception innerException)
        : base(message, innerException)
    {
        SecretName = secretName;
    }
}

/// <summary>
/// Exception thrown when WIQL validation fails.
/// </summary>
public class WIQLValidationException : Exception
{
    public List<string> ValidationErrors { get; }

    public WIQLValidationException(List<string> validationErrors)
        : base($"WIQL validation failed: {string.Join(", ", validationErrors)}")
    {
        ValidationErrors = validationErrors;
    }

    public WIQLValidationException(string message)
        : base(message)
    {
        ValidationErrors = new List<string> { message };
    }

    public WIQLValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        ValidationErrors = new List<string> { message };
    }
}

/// <summary>
/// Exception thrown when an attachment exceeds the maximum size limit.
/// </summary>
public class AttachmentTooLargeException : Exception
{
    public long AttachmentSize { get; }
    public long MaxSize { get; }

    public AttachmentTooLargeException(long attachmentSize, long maxSize)
        : base($"Attachment size ({attachmentSize} bytes) exceeds maximum allowed size ({maxSize} bytes).")
    {
        AttachmentSize = attachmentSize;
        MaxSize = maxSize;
    }

    public AttachmentTooLargeException(string message)
        : base(message)
    {
    }

    public AttachmentTooLargeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a Git merge conflict is detected.
/// </summary>
public class MergeConflictException : Exception
{
    public string RepositoryPath { get; }

    public MergeConflictException(string repositoryPath)
        : base($"Merge conflict detected in repository: {repositoryPath}")
    {
        RepositoryPath = repositoryPath;
    }

    public MergeConflictException(string repositoryPath, string message)
        : base(message)
    {
        RepositoryPath = repositoryPath;
    }

    public MergeConflictException(string repositoryPath, string message, Exception innerException)
        : base(message, innerException)
    {
        RepositoryPath = repositoryPath;
    }
}

namespace Phase3.AzureDevOps.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a Git operation fails.
/// </summary>
public class GitOperationException : Exception
{
    public GitOperationException(string message) : base(message) { }
    public GitOperationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a synchronization conflict is detected.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}

namespace Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Validates WIQL queries for security and correctness.
/// </summary>
public interface IWIQLValidator
{
    /// <summary>
    /// Validates a WIQL query for SQL injection attempts.
    /// </summary>
    /// <param name="wiql">The WIQL query to validate.</param>
    /// <returns>True if the query is valid; false otherwise.</returns>
    bool IsValid(string wiql);
    
    /// <summary>
    /// Validates a WIQL query and throws an exception if invalid.
    /// </summary>
    /// <param name="wiql">The WIQL query to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the query contains invalid patterns.</exception>
    void ValidateOrThrow(string wiql);
    
    /// <summary>
    /// Validates a WIQL query and returns detailed validation result.
    /// </summary>
    /// <param name="wiql">The WIQL query to validate.</param>
    /// <returns>Validation result with errors if validation fails.</returns>
    WIQLValidationResult Validate(string wiql);
}

/// <summary>
/// Result of WIQL validation.
/// </summary>
public class WIQLValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}


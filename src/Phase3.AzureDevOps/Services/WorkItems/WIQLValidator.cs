namespace Phase3.AzureDevOps.Services.WorkItems;

using Phase3.AzureDevOps.Interfaces;
using System.Text.RegularExpressions;

/// <summary>
/// Validates WIQL queries to prevent injection attacks.
/// </summary>
public class WIQLValidator : IWIQLValidator
{
    private static readonly Regex DangerousPatterns = new Regex(
        @"(;\s*(DROP|DELETE|UPDATE|INSERT|ALTER|CREATE|EXEC|EXECUTE)\s+)|" +
        @"(--)|" +
        @"(/\*)|" +
        @"(\*/)|" +
        @"(xp_)|" +
        @"(sp_)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] AllowedClauses = new[]
    {
        "SELECT", "FROM", "WHERE", "ORDER BY", "AND", "OR", "NOT", "IN", "LIKE", "BETWEEN"
    };

    private static readonly string[] AllowedFields = new[]
    {
        "System.Id", "System.Title", "System.State", "System.AssignedTo",
        "System.CreatedDate", "System.ChangedDate", "System.WorkItemType",
        "System.AreaPath", "System.IterationPath", "System.Tags",
        "System.Priority", "System.Description", "System.Reason",
        "System.CreatedBy", "System.ChangedBy", "System.TeamProject",
        "Custom.ProcessingAgent", "Custom.ClaimExpiry"
    };

    /// <inheritdoc />
    public bool IsValid(string wiql)
    {
        var result = Validate(wiql);
        return result.IsValid;
    }

    /// <inheritdoc />
    public void ValidateOrThrow(string wiql)
    {
        var result = Validate(wiql);
        if (!result.IsValid)
        {
            throw new ArgumentException($"Invalid WIQL query: {string.Join(", ", result.Errors)}");
        }
    }

    /// <summary>
    /// Validates a WIQL query.
    /// </summary>
    /// <param name="wiql">The WIQL query to validate.</param>
    /// <returns>Validation result with errors if validation fails.</returns>
    public WIQLValidationResult Validate(string wiql)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(wiql))
        {
            errors.Add("WIQL query cannot be null or empty.");
            return new WIQLValidationResult { IsValid = false, Errors = errors };
        }

        // Check for dangerous patterns
        if (DangerousPatterns.IsMatch(wiql))
        {
            errors.Add("WIQL query contains dangerous patterns (SQL injection attempt detected).");
        }

        // Check for required SELECT clause
        if (!wiql.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("WIQL query must contain a SELECT clause.");
        }

        // Check for required FROM clause
        if (!wiql.Contains("FROM WorkItems", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("WIQL query must contain 'FROM WorkItems' clause.");
        }

        // Validate field names
        var fieldMatches = Regex.Matches(wiql, @"\[([^\]]+)\]");
        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (!AllowedFields.Contains(fieldName) && !fieldName.StartsWith("Custom."))
            {
                errors.Add($"Field '{fieldName}' is not in the allowed fields list.");
            }
        }

        return new WIQLValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

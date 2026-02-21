namespace TownOfUsStatsExporter.Config;

/// <summary>
/// Configuration model for API settings.
/// </summary>
public class ApiConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether API export is enabled.
    /// </summary>
    public bool EnableApiExport { get; set; } = false;

    /// <summary>
    /// Gets or sets the API authentication token.
    /// </summary>
    public string? ApiToken { get; set; } = null;

    /// <summary>
    /// Gets or sets the API endpoint URL.
    /// </summary>
    public string? ApiEndpoint { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether local backups should be saved.
    /// </summary>
    public bool SaveLocalBackup { get; set; } = false;

    /// <summary>
    /// Gets or sets the optional secret for additional authentication.
    /// </summary>
    public string? Secret { get; set; } = null;

    /// <summary>
    /// Checks if the configuration is valid for API export.
    /// </summary>
    /// <returns>True if configuration is valid.</returns>
    public bool IsValid()
    {
        return EnableApiExport
               && !string.IsNullOrWhiteSpace(ApiToken)
               && !string.IsNullOrWhiteSpace(ApiEndpoint);
    }
}

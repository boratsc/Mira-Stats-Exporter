using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TownOfUsStatsExporter.Config;

/// <summary>
/// Manager for reading and writing API configuration.
/// </summary>
public static class ApiConfigManager
{
    private const string ConfigFileName = "ApiSet.ini";

    /// <summary>
    /// Reads the API configuration from disk.
    /// </summary>
    /// <returns>The configuration object.</returns>
    public static async Task<ApiConfig> ReadConfigAsync()
    {
        var config = new ApiConfig();

        try
        {
            foreach (var configPath in GetConfigSearchPaths())
            {
                if (File.Exists(configPath))
                {
                    TownOfUsStatsPlugin.Logger.LogInfo($"Reading config from: {configPath}");
                    var lines = await File.ReadAllLinesAsync(configPath);
                    config = ParseIniFile(lines);
                    TownOfUsStatsPlugin.Logger.LogInfo($"Config loaded: EnableExport={config.EnableApiExport}");
                    return config;
                }
            }

            // No config found - create default
            var defaultPath = GetConfigSearchPaths().Last();
            await CreateDefaultConfigAsync(defaultPath);
            TownOfUsStatsPlugin.Logger.LogWarning($"Config file created at: {defaultPath}");
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error reading config: {ex.Message}");
        }

        return config;
    }

    private static IEnumerable<string> GetConfigSearchPaths()
    {
        // 1. Game directory
        var gameDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        yield return Path.Combine(gameDirectory!, ConfigFileName);

        // 2. Documents/TownOfUs
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var touFolder = Path.Combine(documentsPath, "TownOfUs");
        Directory.CreateDirectory(touFolder);
        yield return Path.Combine(touFolder, ConfigFileName);
    }

    private static ApiConfig ParseIniFile(string[] lines)
    {
        var config = new ApiConfig();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#") || line.Trim().StartsWith(";"))
            {
                continue;
            }

            var parts = line.Split('=', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            switch (key.ToLowerInvariant())
            {
                case "enableapiexport":
                    config.EnableApiExport = bool.TryParse(value, out var enable) && enable;
                    break;

                case "apitoken":
                    if (!string.IsNullOrWhiteSpace(value) && value != "null")
                    {
                        config.ApiToken = value;
                    }

                    break;

                case "apiendpoint":
                    if (!string.IsNullOrWhiteSpace(value) && value != "null")
                    {
                        config.ApiEndpoint = value;
                    }

                    break;

                case "savelocalbackup":
                    config.SaveLocalBackup = bool.TryParse(value, out var save) && save;
                    break;

                case "secret":
                    if (!string.IsNullOrWhiteSpace(value) && value != "null")
                    {
                        config.Secret = value;
                    }

                    break;
            }
        }

        return config;
    }

    private static async Task CreateDefaultConfigAsync(string configPath)
    {
        var defaultConfig = @"# TownOfUs Stats Exporter Configuration
# Whether to enable API export (true/false)
EnableApiExport=false

# API Authentication Token
ApiToken=

# API Endpoint URL
ApiEndpoint=

# Whether to save local backup copies (true/false)
SaveLocalBackup=false

# Additional secret/password for API authentication
Secret=

# Example configuration:
# EnableApiExport=true
# ApiToken=your_secret_token_here
# ApiEndpoint=https://api.example.com/api/among-data
# SaveLocalBackup=true
# Secret=your_secret_key_here
";

        await File.WriteAllTextAsync(configPath, defaultConfig);
    }
}

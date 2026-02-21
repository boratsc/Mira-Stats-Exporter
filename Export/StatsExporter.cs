using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TownOfUsStatsExporter.Config;
using TownOfUsStatsExporter.Models;

namespace TownOfUsStatsExporter.Export;

/// <summary>
/// Main orchestrator for stats export process.
/// </summary>
public static class StatsExporter
{
    /// <summary>
    /// Exports game statistics asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExportGameStatsAsync()
    {
        try
        {
            TownOfUsStatsPlugin.Logger.LogInfo("=== Starting Game Stats Export ===");

            // Read configuration
            var config = await ApiConfigManager.ReadConfigAsync();

            if (!config.EnableApiExport)
            {
                TownOfUsStatsPlugin.Logger.LogInfo("API export is disabled - skipping");
                return;
            }

            if (!config.IsValid())
            {
                TownOfUsStatsPlugin.Logger.LogWarning("API configuration is incomplete - skipping export");
                return;
            }

            // Get data from TOU Mira via reflection
            var bridge = ReflectionBridgeProvider.GetBridge();

            var playerRecords = bridge.GetPlayerRecords();
            if (playerRecords.Count == 0)
            {
                TownOfUsStatsPlugin.Logger.LogWarning("No player data available - skipping export");
                return;
            }

            var playerStats = bridge.GetPlayerStats();
            var roleHistory = bridge.GetRoleHistory();
            var killedPlayers = bridge.GetKilledPlayers();
            var winningFaction = bridge.GetWinningFaction();

            TownOfUsStatsPlugin.Logger.LogInfo($"Collected data: {playerRecords.Count} players, {playerStats.Count} stats entries");

            // Transform to export format
            var gameData = DataTransformer.TransformToExportFormat(
                playerRecords,
                playerStats,
                roleHistory,
                killedPlayers,
                winningFaction,
                config.ApiToken!,
                config.Secret);

            TownOfUsStatsPlugin.Logger.LogInfo($"Transformed data: {gameData.Players.Count} players ready for export");

            // Save local backup if enabled
            if (config.SaveLocalBackup)
            {
                await SaveLocalBackupAsync(gameData);
            }

            // Send to API
            await ApiClient.SendToApiAsync(gameData, config.ApiEndpoint!);

            TownOfUsStatsPlugin.Logger.LogInfo("=== Game Stats Export Completed Successfully ===");
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error during stats export: {ex}");
        }
    }

    private static async Task SaveLocalBackupAsync(GameStatsData data)
    {
        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var logFolder = Path.Combine(documentsPath, "TownOfUs", "GameLogs");
            Directory.CreateDirectory(logFolder);

            var gameIdShort = data.GameInfo.GameId.Substring(0, 8);
            var fileName = $"Game_{DateTime.Now:yyyyMMdd_HHmmss}_{gameIdShort}.json";
            var filePath = Path.Combine(logFolder, fileName);

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            };

            var jsonData = JsonSerializer.Serialize(data, jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonData, Encoding.UTF8);

            TownOfUsStatsPlugin.Logger.LogInfo($"Local backup saved: {filePath}");
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Failed to save local backup: {ex}");
        }
    }
}

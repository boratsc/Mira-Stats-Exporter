using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TownOfUsStatsExporter.Models;
using UnityEngine;

namespace TownOfUsStatsExporter.Export;

/// <summary>
/// Transforms reflected TOU Mira data into export format.
/// </summary>
public static class DataTransformer
{
    private static readonly Dictionary<byte, string> MapNames = new()
    {
        { 0, "The Skeld" },
        { 1, "MIRA HQ" },
        { 2, "Polus" },
        { 3, "Airship" },
        { 4, "The Fungle" },
        { 5, "Submerged" },
    };

    /// <summary>
    /// Transforms TOU Mira data to export format.
    /// </summary>
    /// <param name="playerRecords">Player records from TOU Mira.</param>
    /// <param name="playerStats">Player statistics from TOU Mira.</param>
    /// <param name="roleHistory">Role history from TOU Mira.</param>
    /// <param name="killedPlayers">Killed players list from TOU Mira.</param>
    /// <param name="winningFaction">Winning faction name.</param>
    /// <param name="apiToken">API authentication token.</param>
    /// <param name="secret">Optional secret for authentication.</param>
    /// <returns>Game statistics data ready for export.</returns>
    public static GameStatsData TransformToExportFormat(
        List<PlayerRecordData> playerRecords,
        Dictionary<byte, PlayerStatsData> playerStats,
        Dictionary<byte, List<string>> roleHistory,
        List<KilledPlayerData> killedPlayers,
        string winningFaction,
        string apiToken,
        string? secret)
    {
        var gameData = new GameStatsData
        {
            Token = apiToken,
            Secret = secret,
            GameInfo = BuildGameInfo(),
            GameResult = new GameResultData
            {
                WinningTeam = DetermineWinningTeam(winningFaction, playerRecords),
            },
        };

        // Transform each player
        foreach (var record in playerRecords)
        {
            try
            {
                var playerData = TransformPlayerData(record, playerStats, roleHistory, killedPlayers);
                gameData.Players.Add(playerData);
            }
            catch (Exception ex)
            {
                TownOfUsStatsPlugin.Logger.LogError($"Error transforming player {record.PlayerName}: {ex}");
            }
        }

        return gameData;
    }

    private static GameInfoData BuildGameInfo()
    {
        return new GameInfoData
        {
            GameId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            LobbyCode = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId),
            GameMode = GameOptionsManager.Instance?.CurrentGameOptions?.GameMode.ToString() ?? "Unknown",
            Duration = Time.time,
            Map = GetMapName((byte)(GameOptionsManager.Instance?.CurrentGameOptions?.MapId ?? 0)),
        };
    }

    private static PlayerExportData TransformPlayerData(
        PlayerRecordData record,
        Dictionary<byte, PlayerStatsData> playerStats,
        Dictionary<byte, List<string>> roleHistory,
        List<KilledPlayerData> killedPlayers)
    {
        var player = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.PlayerId == record.PlayerId);

        // Get role history for this player
        var roles = roleHistory.GetValueOrDefault(record.PlayerId, new List<string>());
        
        // If roleHistory is empty, try parsing from RoleString as fallback
        if (roles.Count == 0 && !string.IsNullOrEmpty(record.RoleString))
        {
            TownOfUsStatsPlugin.Logger.LogInfo($"RoleHistory empty for player {record.PlayerId}, parsing from RoleString: {record.RoleString}");
            roles = ParseRolesFromRoleString(record.RoleString);
        }
        
        var lastRole = roles.LastOrDefault() ?? "Unknown";

        // Get stats
        var stats = playerStats.GetValueOrDefault(record.PlayerId, new PlayerStatsData());

        // Count kills
        var kills = killedPlayers.Count(k => k.KillerId == record.PlayerId && k.VictimId != record.PlayerId);

        // Get modifiers
        var bridge = ReflectionBridgeProvider.GetBridge();
        var modifiers = bridge.GetPlayerModifiers(record.PlayerId);
        
        // If no modifiers found via reflection, try parsing from RoleString
        if (modifiers.Count == 0 && !string.IsNullOrEmpty(record.RoleString))
        {
            modifiers = ParseModifiersFromRoleString(record.RoleString);
            if (modifiers.Count > 0)
            {
                TownOfUsStatsPlugin.Logger.LogInfo($"Parsed {modifiers.Count} modifier(s) from RoleString for player {record.PlayerId}");
            }
        }

        // Get task info
        int totalTasks = 0;
        int completedTasks = 0;
        if (player != null && player.Data?.Tasks != null)
        {
            totalTasks = player.Data.Tasks.ToArray().Length;
            completedTasks = player.Data.Tasks.ToArray().Count(t => t.Complete);
        }

        // Fix assassin kills: negative values mean incorrect guesses
        // TOU Mira uses CorrectAssassinKills-- when player misguesses, resulting in -1
        int correctAssassinKills = stats.CorrectAssassinKills;
        int incorrectAssassinKills = stats.IncorrectAssassinKills;
        
        if (correctAssassinKills < 0)
        {
            // Negative correct kills means they misguessed
            incorrectAssassinKills += Math.Abs(correctAssassinKills);
            correctAssassinKills = 0;
        }

        return new PlayerExportData
        {
            PlayerId = record.PlayerId,
            PlayerName = StripColorTags(record.PlayerName),
            PlayerTag = GetPlayerTag(player),
            Platform = GetPlayerPlatform(player),
            Role = lastRole,
            Roles = roles,
            Modifiers = modifiers,
            IsWinner = record.Winner,
            Stats = new PlayerStatsNumbers
            {
                TotalTasks = totalTasks,
                TasksCompleted = completedTasks,
                Kills = kills,
                CorrectKills = stats.CorrectKills,
                IncorrectKills = stats.IncorrectKills,
                CorrectAssassinKills = correctAssassinKills,
                IncorrectAssassinKills = incorrectAssassinKills,
            },
        };
    }

    private static string DetermineWinningTeam(string winningFaction, List<PlayerRecordData> playerRecords)
    {
        // Use WinningFaction from GameHistory if available
        if (!string.IsNullOrEmpty(winningFaction))
        {
            return winningFaction;
        }

        // Fallback: Check first winner's team
        var winner = playerRecords.FirstOrDefault(r => r.Winner);
        if (winner == null)
        {
            return "Unknown";
        }

        return winner.TeamString switch
        {
            "Crewmate" => "Crewmates",
            "Impostor" => "Impostors",
            "Neutral" => "Neutrals",
            "Custom" => "Custom",
            _ => "Unknown",
        };
    }

    private static string GetMapName(byte mapId)
    {
        return MapNames.TryGetValue(mapId, out var name) ? name : $"Unknown Map ({mapId})";
    }

    private static string StripColorTags(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = Regex.Replace(text, @"<color=#[A-Fa-f0-9]+>", string.Empty);
        text = text.Replace("</color>", string.Empty);
        return text.Trim();
    }

    private static string? GetPlayerTag(PlayerControl? player)
    {
        if (player?.Data?.FriendCode != null && !string.IsNullOrEmpty(player.Data.FriendCode))
        {
            return player.Data.FriendCode;
        }

        return null;
    }

    private static string GetPlayerPlatform(PlayerControl? player)
    {
        if (player?.Data != null)
        {
            // Try to get platform info - may not be available in all Among Us versions
            try
            {
                var platformField = player.Data.GetType().GetField("Platform");
                if (platformField != null)
                {
                    var platformValue = platformField.GetValue(player.Data);
                    if (platformValue != null)
                    {
                        return platformValue.ToString() ?? "Unknown";
                    }
                }

                var platformProperty = player.Data.GetType().GetProperty("Platform");
                if (platformProperty != null)
                {
                    var platformValue = platformProperty.GetValue(player.Data);
                    if (platformValue != null)
                    {
                        return platformValue.ToString() ?? "Unknown";
                    }
                }
            }
            catch
            {
                // Platform not available, continue
            }
        }

        return "Unknown";
    }

    /// <summary>
    /// Parses roles from RoleString format with color tags and separators.
    /// </summary>
    private static List<string> ParseRolesFromRoleString(string roleString)
    {
        var roles = new List<string>();
        
        if (string.IsNullOrEmpty(roleString))
        {
            return roles;
        }

        // RoleString format: "RoleName (Modifier) (0/4) | Status | Other Info"
        // We only want the role names before " > " separator
        
        // First, split by " > " to get role history
        var roleParts = roleString.Split(new[] { " > " }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in roleParts)
        {
            // Strip color tags first
            var cleanPart = StripColorTags(part).Trim();
            
            // Extract just the role name before any modifiers or additional info
            // Format: "RoleName (Modifier) (Tasks) | Other..."
            
            // Remove everything after " | " (status info like "Alive", "Killed By", etc.)
            var pipeIndex = cleanPart.IndexOf(" |");
            if (pipeIndex > 0)
            {
                cleanPart = cleanPart.Substring(0, pipeIndex).Trim();
            }
            
            // Remove task info like "(0/4)" at the end
            cleanPart = Regex.Replace(cleanPart, @"\s*\(\d+/\d+\)\s*$", "").Trim();
            
            // Remove modifier info in parentheses like "(Flash)", "(Button Barry)"
            // Keep only the first part before parentheses
            var parenIndex = cleanPart.IndexOf('(');
            if (parenIndex > 0)
            {
                cleanPart = cleanPart.Substring(0, parenIndex).Trim();
            }
            
            if (!string.IsNullOrEmpty(cleanPart))
            {
                roles.Add(cleanPart);
            }
        }
        
        return roles;
    }

    /// <summary>
    /// Parses modifiers from RoleString format.
    /// Modifiers appear in parentheses after the role name.
    /// Example: "Undertaker (Button Barry)" -> ["Button Barry"]
    /// </summary>
    private static List<string> ParseModifiersFromRoleString(string roleString)
    {
        var modifiers = new List<string>();
        
        if (string.IsNullOrEmpty(roleString))
        {
            return modifiers;
        }

        // Strip color tags first
        var cleanString = StripColorTags(roleString);
        
        // Remove everything after " | " (status info)
        var pipeIndex = cleanString.IndexOf(" |");
        if (pipeIndex > 0)
        {
            cleanString = cleanString.Substring(0, pipeIndex).Trim();
        }
        
        // Remove task info like "(0/4)" at the end
        cleanString = Regex.Replace(cleanString, @"\s*\(\d+/\d+\)\s*$", "").Trim();
        
        // Now extract modifiers from parentheses
        // Pattern: RoleName (Modifier1, Modifier2) or RoleName (Modifier)
        var modifierPattern = @"\(([^)]+)\)";
        var matches = Regex.Matches(cleanString, modifierPattern);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var modifierText = match.Groups[1].Value.Trim();
                
                // Split by comma if there are multiple modifiers
                var modifierNames = modifierText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var modName in modifierNames)
                {
                    var cleanModifier = modName.Trim();
                    if (!string.IsNullOrEmpty(cleanModifier))
                    {
                        modifiers.Add(cleanModifier);
                    }
                }
            }
        }
        
        return modifiers;
    }
}

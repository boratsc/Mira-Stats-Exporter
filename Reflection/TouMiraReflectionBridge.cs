using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TownOfUsStatsExporter.Models;

namespace TownOfUsStatsExporter.Reflection;

/// <summary>
/// Main bridge for accessing TOU Mira data through reflection.
/// Caches all reflection metadata for performance.
/// </summary>
public class TouMiraReflectionBridge
{
    private Assembly? touAssembly;
    private readonly ReflectionCache cache = new();

    /// <summary>
    /// Gets the TOU Mira version.
    /// </summary>
    public string? TouMiraVersion { get; private set; }

    /// <summary>
    /// Gets the compatibility status message.
    /// </summary>
    public string CompatibilityStatus { get; private set; } = "Unknown";

    /// <summary>
    /// Initialize the reflection bridge by finding TOU Mira and caching reflection metadata.
    /// </summary>
    /// <returns>True if initialization was successful.</returns>
    public bool Initialize()
    {
        try
        {
            TownOfUsStatsPlugin.Logger.LogInfo("Initializing TOU Mira reflection bridge...");

            // Find TOU Mira assembly - try multiple possible names
            var possibleNames = new[] { "TownOfUs", "TownOfUsMira", "TownOfUs.dll" };

            foreach (var name in possibleNames)
            {
                touAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == name || a.GetName().Name?.Contains(name) == true);

                if (touAssembly != null)
                {
                    TownOfUsStatsPlugin.Logger.LogInfo($"Found TOU Mira assembly: {touAssembly.GetName().Name}");
                    break;
                }
            }

            if (touAssembly == null)
            {
                // Log all loaded assemblies for debugging
                var allAssemblies = string.Join(", ", AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetName().Name)
                    .Where(n => n != null && (n.Contains("Town") || n.Contains("Mira")))
                    .ToArray());

                TownOfUsStatsPlugin.Logger.LogError($"TOU Mira assembly not found! Available assemblies with 'Town' or 'Mira': {allAssemblies}");
                return false;
            }

            TouMiraVersion = touAssembly.GetName().Version?.ToString() ?? "Unknown";
            TownOfUsStatsPlugin.Logger.LogInfo($"Found TOU Mira assembly v{TouMiraVersion}");

            // Check version compatibility
            CompatibilityStatus = VersionCompatibility.CheckVersion(TouMiraVersion);
            if (CompatibilityStatus.StartsWith("Unsupported"))
            {
                TownOfUsStatsPlugin.Logger.LogWarning($"Version compatibility: {CompatibilityStatus}");
                TownOfUsStatsPlugin.Logger.LogWarning("Plugin may not work correctly!");
            }

            // Cache reflection metadata
            if (!CacheReflectionMetadata())
            {
                TownOfUsStatsPlugin.Logger.LogError("Failed to cache reflection metadata");
                return false;
            }

            TownOfUsStatsPlugin.Logger.LogInfo("Reflection bridge initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Failed to initialize reflection bridge: {ex}");
            return false;
        }
    }

    private bool CacheReflectionMetadata()
    {
        try
        {
            // Find and cache EndGamePatches type
            cache.EndGamePatchesType = touAssembly!.GetType("TownOfUs.Patches.EndGamePatches");
            if (cache.EndGamePatchesType == null)
            {
                TownOfUsStatsPlugin.Logger.LogError("Type not found: TownOfUs.Patches.EndGamePatches");
                return false;
            }

            // Find and cache EndGameData nested type
            cache.EndGameDataType = cache.EndGamePatchesType.GetNestedType("EndGameData", BindingFlags.Public);
            if (cache.EndGameDataType == null)
            {
                TownOfUsStatsPlugin.Logger.LogError("Type not found: EndGameData");
                return false;
            }

            // Find and cache PlayerRecord nested type
            cache.PlayerRecordType = cache.EndGameDataType.GetNestedType("PlayerRecord", BindingFlags.Public);
            if (cache.PlayerRecordType == null)
            {
                TownOfUsStatsPlugin.Logger.LogError("Type not found: PlayerRecord");
                return false;
            }

            // Cache PlayerRecords property
            cache.PlayerRecordsProperty = cache.EndGameDataType.GetProperty(
                "PlayerRecords",
                BindingFlags.Public | BindingFlags.Static);
            if (cache.PlayerRecordsProperty == null)
            {
                TownOfUsStatsPlugin.Logger.LogError("Property not found: EndGameData.PlayerRecords");
                return false;
            }

            // Find and cache GameHistory type
            cache.GameHistoryType = touAssembly.GetType("TownOfUs.Modules.GameHistory");
            if (cache.GameHistoryType == null)
            {
                TownOfUsStatsPlugin.Logger.LogError("Type not found: TownOfUs.Modules.GameHistory");
                return false;
            }

            // Cache GameHistory fields (they are fields, not properties!)
            cache.PlayerStatsField = cache.GameHistoryType.GetField(
                "PlayerStats",
                BindingFlags.Public | BindingFlags.Static);
            cache.RoleHistoryField = cache.GameHistoryType.GetField(
                "RoleHistory",
                BindingFlags.Public | BindingFlags.Static);
            cache.KilledPlayersField = cache.GameHistoryType.GetField(
                "KilledPlayers",
                BindingFlags.Public | BindingFlags.Static);
            cache.WinningFactionField = cache.GameHistoryType.GetField(
                "WinningFaction",
                BindingFlags.Public | BindingFlags.Static);

            if (cache.PlayerStatsField == null || cache.RoleHistoryField == null)
            {
                TownOfUsStatsPlugin.Logger.LogError("Required GameHistory fields not found");
                return false;
            }

            TownOfUsStatsPlugin.Logger.LogInfo("All required types and properties cached successfully");
            return true;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error caching reflection metadata: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Get player records from EndGameData.
    /// </summary>
    /// <returns>List of player record data.</returns>
    public List<PlayerRecordData> GetPlayerRecords()
    {
        try
        {
            TownOfUsStatsPlugin.Logger.LogInfo("Getting player records from EndGameData...");
            var playerRecords = cache.PlayerRecordsProperty!.GetValue(null);
            if (playerRecords == null)
            {
                TownOfUsStatsPlugin.Logger.LogWarning("PlayerRecords is null");
                return new List<PlayerRecordData>();
            }

            TownOfUsStatsPlugin.Logger.LogInfo($"PlayerRecords object retrieved: {playerRecords.GetType().Name}");

            // Handle IL2CPP list
            var recordsList = IL2CPPHelper.ConvertToManagedList(playerRecords);
            TownOfUsStatsPlugin.Logger.LogInfo($"Converted to managed list: {recordsList.Count} items");
            var result = new List<PlayerRecordData>();

            foreach (var record in recordsList)
            {
                if (record == null)
                {
                    continue;
                }

                result.Add(new PlayerRecordData
                {
                    PlayerName = GetPropertyValue<string>(record, "PlayerName") ?? "Unknown",
                    RoleString = GetPropertyValue<string>(record, "RoleString") ?? string.Empty,
                    Winner = GetPropertyValue<bool>(record, "Winner"),
                    PlayerId = GetPropertyValue<byte>(record, "PlayerId"),
                    TeamString = GetTeamString(record),
                });
            }

            TownOfUsStatsPlugin.Logger.LogInfo($"Retrieved {result.Count} player records");
            return result;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error getting player records: {ex}");
            return new List<PlayerRecordData>();
        }
    }

    /// <summary>
    /// Get player statistics from GameHistory.
    /// </summary>
    /// <returns>Dictionary of player stats keyed by player ID.</returns>
    public Dictionary<byte, PlayerStatsData> GetPlayerStats()
    {
        try
        {
            var playerStats = cache.PlayerStatsField!.GetValue(null);
            if (playerStats == null)
            {
                TownOfUsStatsPlugin.Logger.LogWarning("PlayerStats is null");
                return new Dictionary<byte, PlayerStatsData>();
            }

            var statsDict = (IDictionary)playerStats;
            var result = new Dictionary<byte, PlayerStatsData>();

            foreach (DictionaryEntry entry in statsDict)
            {
                var playerId = (byte)entry.Key;
                var stats = entry.Value;

                if (stats == null)
                {
                    continue;
                }

                result[playerId] = new PlayerStatsData
                {
                    CorrectKills = GetPropertyValue<int>(stats, "CorrectKills"),
                    IncorrectKills = GetPropertyValue<int>(stats, "IncorrectKills"),
                    CorrectAssassinKills = GetPropertyValue<int>(stats, "CorrectAssassinKills"),
                    IncorrectAssassinKills = GetPropertyValue<int>(stats, "IncorrectAssassinKills"),
                };
            }

            TownOfUsStatsPlugin.Logger.LogInfo($"Retrieved stats for {result.Count} players");
            return result;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error getting player stats: {ex}");
            return new Dictionary<byte, PlayerStatsData>();
        }
    }

    /// <summary>
    /// Get role history from GameHistory.
    /// </summary>
    /// <returns>Dictionary of role lists keyed by player ID.</returns>
    public Dictionary<byte, List<string>> GetRoleHistory()
    {
        try
        {
            TownOfUsStatsPlugin.Logger.LogInfo("Getting role history from GameHistory...");
            var roleHistory = cache.RoleHistoryField!.GetValue(null);
            if (roleHistory == null)
            {
                TownOfUsStatsPlugin.Logger.LogWarning("RoleHistory is null");
                return new Dictionary<byte, List<string>>();
            }

            var historyList = IL2CPPHelper.ConvertToManagedList(roleHistory);
            TownOfUsStatsPlugin.Logger.LogInfo($"RoleHistory has {historyList.Count} entries");
            var result = new Dictionary<byte, List<string>>();

            foreach (var entry in historyList)
            {
                if (entry == null)
                {
                    TownOfUsStatsPlugin.Logger.LogWarning("Null entry in RoleHistory");
                    continue;
                }

                // Entry is KeyValuePair<byte, RoleBehaviour>
                var kvpType = entry.GetType();
                TownOfUsStatsPlugin.Logger.LogInfo($"Entry type: {kvpType.Name}");
                
                var keyProp = kvpType.GetProperty("Key");
                var valueProp = kvpType.GetProperty("Value");
                
                if (keyProp == null || valueProp == null)
                {
                    TownOfUsStatsPlugin.Logger.LogError($"Could not find Key or Value properties on {kvpType.Name}");
                    continue;
                }
                
                var playerId = (byte)keyProp.GetValue(entry)!;
                var roleBehaviour = valueProp.GetValue(entry);

                if (roleBehaviour == null)
                {
                    TownOfUsStatsPlugin.Logger.LogWarning($"Null RoleBehaviour for player {playerId}");
                    continue;
                }

                TownOfUsStatsPlugin.Logger.LogInfo($"Player {playerId}: RoleBehaviour type = {roleBehaviour.GetType().Name}");

                // Get role name from RoleBehaviour.GetRoleName()
                var getRoleNameMethod = roleBehaviour.GetType().GetMethod("GetRoleName");
                if (getRoleNameMethod == null)
                {
                    TownOfUsStatsPlugin.Logger.LogWarning($"GetRoleName method not found on {roleBehaviour.GetType().Name}");
                    continue;
                }

                var roleName = getRoleNameMethod.Invoke(roleBehaviour, null) as string;
                if (string.IsNullOrEmpty(roleName))
                {
                    TownOfUsStatsPlugin.Logger.LogWarning($"GetRoleName returned null/empty for player {playerId}");
                    continue;
                }

                TownOfUsStatsPlugin.Logger.LogInfo($"Player {playerId}: Role = {roleName}");

                // Skip ghost roles
                if (roleName.Contains("Ghost"))
                {
                    TownOfUsStatsPlugin.Logger.LogInfo($"Skipping ghost role: {roleName}");
                    continue;
                }

                // Strip color tags
                roleName = StripColorTags(roleName);

                if (!result.ContainsKey(playerId))
                {
                    result[playerId] = new List<string>();
                }

                result[playerId].Add(roleName);
            }

            TownOfUsStatsPlugin.Logger.LogInfo($"Retrieved role history for {result.Count} players");
            return result;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error getting role history: {ex}");
            return new Dictionary<byte, List<string>>();
        }
    }

    /// <summary>
    /// Get killed players list.
    /// </summary>
    /// <returns>List of killed player data.</returns>
    public List<KilledPlayerData> GetKilledPlayers()
    {
        try
        {
            var killedPlayers = cache.KilledPlayersField?.GetValue(null);
            if (killedPlayers == null)
            {
                return new List<KilledPlayerData>();
            }

            var killedList = IL2CPPHelper.ConvertToManagedList(killedPlayers);
            var result = new List<KilledPlayerData>();

            foreach (var killed in killedList)
            {
                if (killed == null)
                {
                    continue;
                }

                result.Add(new KilledPlayerData
                {
                    KillerId = GetPropertyValue<byte>(killed, "KillerId"),
                    VictimId = GetPropertyValue<byte>(killed, "VictimId"),
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error getting killed players: {ex}");
            return new List<KilledPlayerData>();
        }
    }

    /// <summary>
    /// Get winning faction string.
    /// </summary>
    /// <returns>The winning faction name.</returns>
    public string GetWinningFaction()
    {
        try
        {
            if (cache.WinningFactionField == null)
            {
                return string.Empty;
            }

            var winningFaction = cache.WinningFactionField.GetValue(null);
            return winningFaction as string ?? string.Empty;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error getting winning faction: {ex}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Get modifiers for a player.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <returns>List of modifier names.</returns>
    public List<string> GetPlayerModifiers(byte playerId)
    {
        try
        {
            // Find PlayerControl
            var player = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrDefault(p => p.PlayerId == playerId);

            if (player == null)
            {
                return new List<string>();
            }

            // Get modifiers through reflection
            var getModifiersMethod = player.GetType().GetMethods()
                .FirstOrDefault(m => m.Name == "GetModifiers" && m.IsGenericMethod);

            if (getModifiersMethod == null)
            {
                return new List<string>();
            }

            // Find GameModifier type
            var gameModifierType = touAssembly!.GetType("MiraAPI.Modifiers.GameModifier");
            if (gameModifierType == null)
            {
                return new List<string>();
            }

            var genericMethod = getModifiersMethod.MakeGenericMethod(gameModifierType);
            var modifiers = genericMethod.Invoke(player, null);

            if (modifiers == null)
            {
                return new List<string>();
            }

            var modifiersList = IL2CPPHelper.ConvertToManagedList(modifiers);
            var result = new List<string>();

            foreach (var modifier in modifiersList)
            {
                if (modifier == null)
                {
                    continue;
                }

                var modifierName = GetPropertyValue<string>(modifier, "ModifierName");
                if (!string.IsNullOrEmpty(modifierName))
                {
                    result.Add(modifierName);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error getting modifiers for player {playerId}: {ex}");
            return new List<string>();
        }
    }

    private string GetTeamString(object record)
    {
        try
        {
            // Get Team property (ModdedRoleTeams enum)
            var teamProperty = record.GetType().GetProperty("Team");
            if (teamProperty == null)
            {
                return "Unknown";
            }

            var team = teamProperty.GetValue(record);
            if (team == null)
            {
                return "Unknown";
            }

            return team.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private T GetPropertyValue<T>(object obj, string propertyName)
    {
        try
        {
            var property = obj?.GetType().GetProperty(propertyName);
            if (property == null)
            {
                return default!;
            }

            var value = property.GetValue(obj);
            if (value == null)
            {
                return default!;
            }

            return (T)value;
        }
        catch
        {
            return default!;
        }
    }

    private string StripColorTags(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = Regex.Replace(text, @"<color=#[A-Fa-f0-9]+>", string.Empty);
        text = text.Replace("</color>", string.Empty);
        text = text.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
        text = text.Replace("<i>", string.Empty).Replace("</i>", string.Empty);

        return text.Trim();
    }
}

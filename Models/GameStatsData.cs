using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TownOfUsStatsExporter.Models;

/// <summary>
/// Main data structure for game statistics export.
/// </summary>
public class GameStatsData
{
    /// <summary>
    /// Gets or sets the API authentication token.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional secret for additional authentication.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    /// <summary>
    /// Gets or sets the game information.
    /// </summary>
    [JsonPropertyName("gameInfo")]
    public GameInfoData GameInfo { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of player data.
    /// </summary>
    [JsonPropertyName("players")]
    public List<PlayerExportData> Players { get; set; } = new();

    /// <summary>
    /// Gets or sets the game result.
    /// </summary>
    [JsonPropertyName("gameResult")]
    public GameResultData GameResult { get; set; } = new();
}

/// <summary>
/// Game session information.
/// </summary>
public class GameInfoData
{
    /// <summary>
    /// Gets or sets the unique game ID.
    /// </summary>
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the game timestamp.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the lobby code.
    /// </summary>
    [JsonPropertyName("lobbyCode")]
    public string LobbyCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the game mode.
    /// </summary>
    [JsonPropertyName("gameMode")]
    public string GameMode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the game duration in seconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public float Duration { get; set; }

    /// <summary>
    /// Gets or sets the map name.
    /// </summary>
    [JsonPropertyName("map")]
    public string Map { get; set; } = string.Empty;
}

/// <summary>
/// Individual player export data.
/// </summary>
public class PlayerExportData
{
    /// <summary>
    /// Gets or sets the player ID.
    /// </summary>
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the player name.
    /// </summary>
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player tag (friend code).
    /// </summary>
    [JsonPropertyName("playerTag")]
    public string? PlayerTag { get; set; }

    /// <summary>
    /// Gets or sets the player platform.
    /// </summary>
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the player's final role.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of all roles the player had during the game.
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of modifiers the player had.
    /// </summary>
    [JsonPropertyName("modifiers")]
    public List<string> Modifiers { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the player won.
    /// </summary>
    [JsonPropertyName("isWinner")]
    public bool IsWinner { get; set; }

    /// <summary>
    /// Gets or sets the player statistics.
    /// </summary>
    [JsonPropertyName("stats")]
    public PlayerStatsNumbers Stats { get; set; } = new();
}

/// <summary>
/// Numeric statistics for a player.
/// </summary>
public class PlayerStatsNumbers
{
    /// <summary>
    /// Gets or sets the total number of tasks.
    /// </summary>
    [JsonPropertyName("totalTasks")]
    public int TotalTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of completed tasks.
    /// </summary>
    [JsonPropertyName("tasksCompleted")]
    public int TasksCompleted { get; set; }

    /// <summary>
    /// Gets or sets the total number of kills.
    /// </summary>
    [JsonPropertyName("kills")]
    public int Kills { get; set; }

    /// <summary>
    /// Gets or sets the number of correct kills.
    /// </summary>
    [JsonPropertyName("correctKills")]
    public int CorrectKills { get; set; }

    /// <summary>
    /// Gets or sets the number of incorrect kills.
    /// </summary>
    [JsonPropertyName("incorrectKills")]
    public int IncorrectKills { get; set; }

    /// <summary>
    /// Gets or sets the number of correct assassin kills.
    /// </summary>
    [JsonPropertyName("correctAssassinKills")]
    public int CorrectAssassinKills { get; set; }

    /// <summary>
    /// Gets or sets the number of incorrect assassin kills.
    /// </summary>
    [JsonPropertyName("incorrectAssassinKills")]
    public int IncorrectAssassinKills { get; set; }
}

/// <summary>
/// Game result data.
/// </summary>
public class GameResultData
{
    /// <summary>
    /// Gets or sets the winning team name.
    /// </summary>
    [JsonPropertyName("winningTeam")]
    public string WinningTeam { get; set; } = "Unknown";
}
